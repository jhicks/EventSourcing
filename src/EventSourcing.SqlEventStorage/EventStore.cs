using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using EventSourcing.EventStorage;

namespace EventSourcing.SqlEventStorage
{
    public class EventStore : IEventStore
    {
        private const string StoreEventsCommandText = "insert into EventStore (StreamId, EventData) values (@StreamId,@EventData)";
        private const string StoreSnapshotCommandText = "insert into SnapshotStore (SourceId, SnapshotData) values (@SourceId, @SnapshotData)";
        private const string ReplayAllEvents = "select EventData from EventStore where StreamId = @StreamId order by Sequence";
        private const string ReplayEventsFromVersionForwardQuery = "with OrderedEvents as (select [EventData], row_number() over(order by Sequence) as 'RowNumber' from [EventStore] where [StreamId] = @StreamId) select [EventData] from OrderedEvents where RowNumber >= @Version";
        private const string LoadSnapshotQuery = "select top 1 SnapshotData from SnapshotStore where SourceId = @SourceId order by timestamp desc";

        private readonly string _connectionString;
        private readonly IFormatter _formatter;
        private Transaction _transaction;

        public EventStore(string connectionString, IFormatter formatter)
        {
            _connectionString = connectionString;
            _formatter = formatter;
        }

        public void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
        {
            EnsureTransaction();

            var data = new DataTable();
            data.Columns.Add("StreamId").DataType = typeof (Guid);
            data.Columns.Add("EventData").DataType = typeof (byte[]);

            foreach(var @event in stream)
            {
                data.Rows.Add(streamId, Serialize(@event));
            }

            var insertCmd = new SqlCommand(StoreEventsCommandText)
                            {
                                Connection = _transaction.SqlConnection, 
                                Transaction = _transaction.SqlTransaction,
                                UpdatedRowSource = UpdateRowSource.None
                            };

            insertCmd.Parameters.Add("@StreamId",SqlDbType.UniqueIdentifier).SourceColumn = "StreamId";
            insertCmd.Parameters.Add("@EventData",SqlDbType.VarBinary).SourceColumn = "EventData";

            var dataAdapter = new SqlDataAdapter
                              {
                                  InsertCommand = insertCmd,
                                  UpdateBatchSize = 0
                              };

            dataAdapter.Update(data);
        }

        public void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class
        {
            EnsureTransaction();

            var insertCmd = new SqlCommand(StoreSnapshotCommandText)
                            {
                                Connection = _transaction.SqlConnection, 
                                Transaction = _transaction.SqlTransaction
                            };

            insertCmd.Parameters.AddWithValue("@SourceId", sourceId);
            insertCmd.Parameters.AddWithValue("@SnapshotData", Serialize(snapshot));

            insertCmd.ExecuteNonQuery();
        }

        private void EnsureTransaction()
        {
            if(_transaction == null)
            {
                throw new InvalidOperationException("Transaction is required.  Call BeginTransaction");
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
                using(var reader = query.ExecuteReader())
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

        public ITransaction BeginTransaction()
        {
            if(_transaction != null)
            {
                throw new InvalidOperationException("Transaction already in progress");
            }

            var connection = new SqlConnection(_connectionString);
            connection.Open();
            _transaction = new Transaction(connection, () =>
            {
                _transaction = null;
                connection.Close();
            });

            return _transaction;
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
