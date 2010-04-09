using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqliteEventStorage.Tests
{
    [Specification]
    public class WhenStoringEvents : InContextOfTestingTheSqliteEventStore
    {
        private Guid _streamId;
        private string[] _events;

        protected override void SetupDependencies()
        {
            _streamId = Guid.NewGuid();
            _events = new[] { "event 1", "event 2", "event 3", "event 4", "event 5" };

            base.SetupDependencies();
        }

        protected override void When()
        {
            _subjectUnderTest.StoreEvents(_streamId,_events);
        }

        [Then]
        public void ItShouldNotThrowAnException()
        {
            Assert.That(_caughtException, Is.Null);
        }

        [Then]
        public void ItShouldStoreEventsInTheDatabase()
        {
			using (var con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                var query = new SQLiteCommand("select count(*) from EventStore where StreamId = @StreamId", con);
                query.Parameters.AddWithValue("@StreamId", _streamId);
                var eventCount = (long)query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(_events.Length));
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
            }
        }
    }
}