using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    public abstract class InContextOfReplayingEventsFromTheSqlEventStore : InContextOfTestingTheSqlEventStore
    {
        protected Guid _streamId;
        protected IEnumerable<string> _replayedEvents;
        protected string[] _events;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _streamId = Guid.NewGuid();
            _events = new[] {"event 1", "event 2", "event 3", "event 4", "event 5"};

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                var cmd = new SqlCommand("insert into EventStore (StreamId, EventData) values (@StreamId, @EventData)") {Connection = connection, Transaction = trans};

                cmd.Parameters.AddWithValue("@StreamId", _streamId);
                cmd.Parameters.Add("@EventData", SqlDbType.VarBinary);

                foreach (var @event in _events)
                {
                    var stream = new MemoryStream();
                    _formatter.Serialize(stream, @event);
                    cmd.Parameters["@EventData"].Value = stream.ToArray();
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }
        }

        [TearDown]
        public void CleanupDb()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("delete from EventStore where StreamId = @StreamId", con);
                cmd.Parameters.AddWithValue("@StreamId", _streamId);
                cmd.ExecuteNonQuery();
            }
        }

        [Then]
        public void ItShouldNotThrowAnException()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}