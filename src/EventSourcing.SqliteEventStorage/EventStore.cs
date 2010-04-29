using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using EventSourcing.Infrastructure;

namespace EventSourcing.SqliteEventStorage
{
	public class EventStore : IEventStore
	{
		private const string StoreEventCommandText = "insert into EventStore (StreamId, Sequence, EventData) values (@StreamId, @Sequence, @EventData)";
		private const string StoreSnapshotCommandText = "insert into SnapshotStore (SourceId, SnapshotData) values (@SourceId, @SnapshotData)";
		private const string ReplayAllEvents = "select EventData from EventStore where StreamId = @StreamId order by Sequence";
		private const string ReplayEventsFromVersionForwardQuery = "select [EventData] from [EventStore] where [StreamId] = @StreamId and [Sequence] >= @Version order by [Sequence]";
		private const string ReplayEventsFromVersionToVersionQuery = "select [EventData] from [EventStore] where [StreamId] = @StreamId and [Sequence] between @FromVersion and @ToVersion order by [Sequence]";
		private const string ReplayEventsFromPointInTimeForwardQuery = "select [EventData] from [EventStore] where [StreamId] = @StreamId and GeneratedOn >= @FromPointInTime order by Sequence";
		private const string ReplayEventsFromPointInTimeToPointInTimeQuery = "select [EventData] from [EventStore] where [StreamId] = @StreamId and GeneratedOn between @FromPointInTime and @ToPointInTime order by Sequence";
		private const string LoadSnapshotQuery = "select top 1 SnapshotData from SnapshotStore where SourceId = @SourceId order by version desc";
		
		private readonly IFormatter _formatter;
		private readonly SQLiteConnection _connection;

		public EventStore(SQLiteConnection connection, IFormatter formatter)
		{
			_connection = connection;
			_formatter = formatter;
		}

		public void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
		{
			if (stream.Count() == 0)
			{
				return;
			}

			WithTransaction(con =>
			{
				var sequenceCommand = con.CreateCommand();
				sequenceCommand.CommandText = "select max([Sequence]) from [EventStore] where [StreamId] = @StreamId";
				sequenceCommand.Parameters.Add("@StreamId", DbType.Guid);
				sequenceCommand.Parameters["@StreamId"].Value = streamId;

				var sequence = sequenceCommand.ExecuteScalar();
				var sequenceGenerator = new SequenceGenerator(streamId, sequence == DBNull.Value ? null : (int?) sequence);

				var insertCmd = con.CreateCommand();
				insertCmd.CommandText = StoreEventCommandText;
				insertCmd.UpdatedRowSource = UpdateRowSource.None;

				insertCmd.Parameters.Add("@StreamId", DbType.Guid);
				insertCmd.Parameters.Add("@Sequence", DbType.Int32);
				insertCmd.Parameters.Add("@EventData", DbType.Binary);

				if (stream.Count() > 1)
				{
					insertCmd.Parameters["@StreamId"].SourceColumn = "StreamId";
					insertCmd.Parameters["@Sequence"].SourceColumn = "Sequence";
					insertCmd.Parameters["@EventData"].SourceColumn = "EventData";

					var data = new DataTable();
					data.Columns.Add("StreamId").DataType = typeof(Guid);
					data.Columns.Add("Sequence").DataType = typeof(int);
					data.Columns.Add("EventData").DataType = typeof(byte[]);

					foreach (var @event in stream)
					{
						data.Rows.Add(streamId, sequenceGenerator.NextSequence(), Serialize(@event));
					}

					var dataAdapter = new SQLiteDataAdapter
					{
						InsertCommand = insertCmd
					};

					dataAdapter.Update(data);
				}
				else
				{
					insertCmd.Parameters["@StreamId"].Value = streamId;
					insertCmd.Parameters["@Sequence"].Value = sequenceGenerator.NextSequence();
					insertCmd.Parameters["@EventData"].Value = Serialize(stream.ElementAt(0));
					insertCmd.ExecuteNonQuery();
				}
			});
		}

		public void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class
		{
			WithTransaction(con =>
			{
				var insertCmd = con.CreateCommand();
				insertCmd.CommandText = StoreSnapshotCommandText;
				insertCmd.Parameters.AddWithValue("@SourceId", sourceId);
				insertCmd.Parameters.AddWithValue("@SnapshotData", Serialize(snapshot));
				insertCmd.ExecuteNonQuery();
			});
		}

		public IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class
		{
			var queryCmd = new SQLiteCommand(ReplayAllEvents);
			queryCmd.Parameters.AddWithValue("@StreamId", streamId);
			return Replay<TEvent>(queryCmd);
		}

		public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion) where TEvent : class
		{
			var queryCmd = new SQLiteCommand(ReplayEventsFromVersionForwardQuery);
			queryCmd.Parameters.AddWithValue("@StreamId", streamId);
			queryCmd.Parameters.AddWithValue("@Version", fromVersion);
			return Replay<TEvent>(queryCmd);
		}

		public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion, int toVersion) where TEvent : class
		{
			var queryCmd = new SQLiteCommand(ReplayEventsFromVersionToVersionQuery);
			queryCmd.Parameters.AddWithValue("@StreamId", streamId);
			queryCmd.Parameters.AddWithValue("@FromVersion", fromVersion);
			queryCmd.Parameters.AddWithValue("@ToVersion", toVersion);
			return Replay<TEvent>(queryCmd);
		}

		public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime) where TEvent : class
		{
			var queryCmd = new SQLiteCommand(ReplayEventsFromPointInTimeForwardQuery);
			queryCmd.Parameters.AddWithValue("@StreamId", streamId);
			queryCmd.Parameters.AddWithValue("@FromPointInTime", fromPointInTime.UtcDateTime);
			return Replay<TEvent>(queryCmd);
		}

		public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime, DateTimeOffset toPointInTime) where TEvent : class
		{
			var queryCmd = new SQLiteCommand(ReplayEventsFromPointInTimeToPointInTimeQuery);
			queryCmd.Parameters.AddWithValue("@StreamId", streamId);
			queryCmd.Parameters.AddWithValue("@FromPointInTime", fromPointInTime.UtcDateTime);
			queryCmd.Parameters.AddWithValue("@ToPointInTime", toPointInTime.UtcDateTime);
			return Replay<TEvent>(queryCmd);
		}

		public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime, TimeSpan period) where TEvent : class
		{
			var toPointInTime = fromPointInTime + period;
			return Replay<TEvent>(streamId, fromPointInTime, toPointInTime);
		}

		private List<TEvent> Replay<TEvent>(SQLiteCommand command) where TEvent : class
		{
			var results = new List<TEvent>();

			WithConnection(con =>
			{
				command.Connection = con;

				using (var reader = command.ExecuteReader())
				{
					if (reader == null)
					{
						return;
					}

					while (reader.Read())
					{
						var data = ReadBytesIntoBuffer(reader, 0);
						var @event = Deserialize<TEvent>(data);
						results.Add(@event);
					}
				}
			});

			return results;
		}

		private static byte[] ReadBytesIntoBuffer(IDataRecord reader, int ordinal)
		{
			var data = new MemoryStream();
			var buffer = new byte[4096];
			var offset = 0;
			while (true)
			{
				var readCount = reader.GetBytes(ordinal, offset, buffer, 0, buffer.Length);
				data.Write(buffer, 0, (int)readCount);
				offset += (int)readCount;

				if (readCount < buffer.Length)
				{
					break;
				}
			}

			return data.ToArray();
		}

		public TSnapshot LoadSnapshot<TSnapshot>(Guid sourceId) where TSnapshot : class
		{
			TSnapshot snapshot = null;

			WithConnection(con =>
			{
				var query = new SQLiteCommand(LoadSnapshotQuery, con);
				
				using (var reader = query.ExecuteReader())
				{
					if (reader == null || !reader.Read())
					{
						return;
					}

					var data = ReadBytesIntoBuffer(reader, 0);
					snapshot = Deserialize<TSnapshot>(data);
				}
			});

			return snapshot;
		}

		private TObject Deserialize<TObject>(byte[] data)
		{
			return (TObject)_formatter.Deserialize(new MemoryStream(data));
		}

		private byte[] Serialize(object data)
		{
			var stream = new MemoryStream();
			_formatter.Serialize(stream, data);
			return stream.ToArray();
		}

		private void WithConnection(Action<SQLiteConnection> action)
		{
			if (_connection.State == ConnectionState.Closed)
				_connection.Open();

			action(_connection);
		}

		private void WithTransaction(Action<SQLiteConnection> action)
		{
			WithConnection(connection =>
			{
				using(var transaction = connection.BeginTransaction())
				{
					try
					{
						action(connection);
						transaction.Commit();
					}
					catch(Exception ex)
					{
						transaction.Rollback();
						throw;
					}
				}
			});
		}
	}
}
