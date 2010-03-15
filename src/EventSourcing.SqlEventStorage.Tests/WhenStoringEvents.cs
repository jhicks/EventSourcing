using System;
using System.Data.SqlClient;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    [Specification]
    public class WhenStoringEvents : InContextOfTestingTheSqlEventStore
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
            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreEvents(_streamId,_events);
                trans.Commit();
            }
        }

        [Then]
        public void ItShouldNotThrowAnException()
        {
            Assert.That(_caughtException, Is.Null);
        }

        [Then]
        public void ItShouldStoreEventsInTheDatabase()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var query = new SqlCommand("select count(*) from EventStore where StreamId = @StreamId", con);
                query.Parameters.AddWithValue("@StreamId", _streamId);
                var eventCount = (int)query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(_events.Length));
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
    }
}