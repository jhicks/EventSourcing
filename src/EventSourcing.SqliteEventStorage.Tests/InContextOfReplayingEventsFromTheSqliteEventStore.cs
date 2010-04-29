using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqliteEventStorage.Tests
{
    public abstract class InContextOfReplayingEventsFromTheSqliteEventStore : InContextOfTestingTheSqliteEventStore
    {
        protected Guid _streamId;
        protected IEnumerable<string> _replayedEvents;
        protected string[] _events;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _streamId = Guid.NewGuid();
            _events = new[] {"event 1", "event 2", "event 3", "event 4", "event 5"};

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                var cmd = new SQLiteCommand("insert into EventStore (StreamId, Sequence, EventData) values (@StreamId, @Sequence, @EventData)") {Connection = connection, Transaction = trans};

            	var sequenceGenerator = new SequenceGenerator(_streamId);

                cmd.Parameters.AddWithValue("@StreamId", _streamId);
                cmd.Parameters.Add("@Sequence", DbType.Int32);
                cmd.Parameters.Add("@EventData", DbType.Binary);

                foreach (var @event in _events)
                {
                    var stream = new MemoryStream();
                    _formatter.Serialize(stream, @event);
                	cmd.Parameters["@Sequence"].Value = sequenceGenerator.NextSequence();
                    cmd.Parameters["@EventData"].Value = stream.ToArray();
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }
        }

        [TearDown]
        public void CleanupDb()
        {
            using (var con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                var cmd = new SQLiteCommand("delete from EventStore where StreamId = @StreamId", con);
                cmd.Parameters.AddWithValue("@StreamId", _streamId);
                cmd.ExecuteNonQuery();
				con.Close();
            }
        }

        [Then]
        public void ItShouldNotThrowAnException()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}