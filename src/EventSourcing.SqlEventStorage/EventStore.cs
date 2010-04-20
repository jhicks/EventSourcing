using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using EventSourcing.EventStorage;

namespace EventSourcing.SqlEventStorage
{
    public class EventStore : IEventStore
    {
        private const string StoreEventCommandText = "insert into EventStore (StreamId, EventData) values (@StreamId,@EventData)";
        private const string StoreSnapshotCommandText = "insert into SnapshotStore (SourceId, SnapshotData) values (@SourceId, @SnapshotData)";
        private const string ReplayAllEvents = "select EventData from EventStore where StreamId = @StreamId order by Sequence";
        private const string ReplayEventsFromVersionForwardQuery = "with OrderedEvents as (select [EventData], row_number() over(order by Sequence) as 'RowNumber' from [EventStore] where [StreamId] = @StreamId) select [EventData] from OrderedEvents where RowNumber >= @Version";
        private const string ReplayEventsFromVersionToVersionQuery = "with OrderedEvents as (select [EventData], row_number() over(order by Sequence) as 'RowNumber' from [EventStore] where [StreamId] = @StreamId) select [EventData] from OrderedEvents where RowNumber between @FromVersion and @ToVersion";
        private const string ReplayEventsFromPointInTimeForwardQuery = "select [EventData] from [EventStore] where [StreamId] = @StreamId and GeneratedOn >= @FromPointInTime order by Sequence";
        private const string ReplayEventsFromPointInTimeToPointInTimeQuery = "select [EventData] from [EventStore] where [StreamId] = @StreamId and GeneratedOn between @FromPointInTime and @ToPointInTime order by Sequence";
        private const string LoadSnapshotQuery = "select top 1 SnapshotData from SnapshotStore where SourceId = @SourceId order by version desc";

        private readonly string _connectionString;
        private readonly IFormatter _formatter;

        public EventStore(string connectionString, IFormatter formatter)
        {
            _connectionString = connectionString;
            _formatter = formatter;
        }

        public void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
        {
            if(stream.Count() == 0)
            {
                return;
            }

            using(var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var insertCmd = con.CreateCommand();
                insertCmd.CommandText = StoreEventCommandText;
                insertCmd.UpdatedRowSource = UpdateRowSource.None;


                insertCmd.Parameters.Add("@StreamId", SqlDbType.UniqueIdentifier);
                insertCmd.Parameters.Add("@EventData", SqlDbType.VarBinary);

                insertCmd.Parameters["@StreamId"].SourceColumn = "StreamId";
                insertCmd.Parameters["@EventData"].SourceColumn = "EventData";

                var data = new DataTable();
                data.Columns.Add("StreamId").DataType = typeof(Guid);
                data.Columns.Add("EventData").DataType = typeof(byte[]);

                foreach (var @event in stream)
                {
                    data.Rows.Add(streamId, Serialize(@event));
                }

                var dataAdapter = new SqlDataAdapter
                                  {
                                      InsertCommand = insertCmd,
                                      UpdateBatchSize = stream.Count()
                                  };

                dataAdapter.Update(data);
                con.Close();
            }
        }

        public void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class
        {
            using(var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var insertCmd = con.CreateCommand();
                insertCmd.CommandText = StoreSnapshotCommandText;
                insertCmd.Parameters.AddWithValue("@SourceId", sourceId);
                insertCmd.Parameters.AddWithValue("@SnapshotData", Serialize(snapshot));
                insertCmd.ExecuteNonQuery();

                con.Close();
            }
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class
        {
                var queryCmd = new SqlCommand(ReplayAllEvents);
                queryCmd.Parameters.AddWithValue("@StreamId", streamId);
                return Replay<TEvent>(queryCmd);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion) where TEvent : class
        {
            var queryCmd = new SqlCommand(ReplayEventsFromVersionForwardQuery);
            queryCmd.Parameters.AddWithValue("@StreamId",streamId);
            queryCmd.Parameters.AddWithValue("@Version", fromVersion);
            return Replay<TEvent>(queryCmd);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion, int toVersion) where TEvent : class
        {
            var queryCmd = new SqlCommand(ReplayEventsFromVersionToVersionQuery);
            queryCmd.Parameters.AddWithValue("@StreamId", streamId);
            queryCmd.Parameters.AddWithValue("@FromVersion", fromVersion);
            queryCmd.Parameters.AddWithValue("@ToVersion", toVersion);
            return Replay<TEvent>(queryCmd);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime) where TEvent : class
        {
            var queryCmd = new SqlCommand(ReplayEventsFromPointInTimeForwardQuery);
            queryCmd.Parameters.AddWithValue("@StreamId", streamId);
            queryCmd.Parameters.AddWithValue("@FromPointInTime", fromPointInTime.UtcDateTime);
            return Replay<TEvent>(queryCmd);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime, DateTimeOffset toPointInTime) where TEvent : class
        {
            var queryCmd = new SqlCommand(ReplayEventsFromPointInTimeToPointInTimeQuery);
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

        private List<TEvent> Replay<TEvent>(SqlCommand command) where TEvent : class
        {
            using(var con = new SqlConnection(_connectionString))
            {
                con.Open();
                command.Connection = con;
                using (var reader = command.ExecuteReader())
                {
                    var results = new List<TEvent>();

                    if (reader == null)
                    {
                        return results;
                    }

                    while (reader.Read())
                    {
                        var data = ReadBytesIntoBuffer(reader,0);
                        var @event = Deserialize<TEvent>(data);
                        results.Add(@event);
                    }

                    return results;
                }
            }
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
            using(var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var query = new SqlCommand(LoadSnapshotQuery, con);
                query.Parameters.AddWithValue("@SourceId", sourceId);
                using (var reader = query.ExecuteReader())
                {
                    if(reader == null || !reader.Read())
                    {
                        return null;
                    }

                    var data = ReadBytesIntoBuffer(reader, 0);
                    return Deserialize<TSnapshot>(data);
                }
            }
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
    }
}
