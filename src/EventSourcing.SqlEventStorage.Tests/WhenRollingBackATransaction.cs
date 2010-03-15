using System;
using System.Data.SqlClient;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    [Specification]
    public class WhenRollingBackATransaction : InContextOfTestingTheSqlEventStore
    {
        private Transaction _transaction;
        private Guid _streamId;
        private Guid _sourceId;
        protected override void SetupDependencies()
        {
            base.SetupDependencies();

            _transaction = _subjectUnderTest.BeginTransaction() as Transaction;

            _streamId = Guid.NewGuid();
            _sourceId = Guid.NewGuid();

            _subjectUnderTest.StoreEvents(_streamId, new[] {"event 1"});
            _subjectUnderTest.StoreSnapshot(_sourceId, "snapshot");
        }

        protected override void When()
        {
            _transaction.Rollback();
        }

        [Then]
        public void ItShouldNotStoreEventsInTheDatabase()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var query = new SqlCommand("select count(*) from EventStore where StreamId = @StreamId", con);
                query.Parameters.AddWithValue("@StreamId", _streamId);
                var eventCount = (int)query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(0));
            }
        }

        [Then]
        public void ItShouldNotStoreSnapshotInTheDatabase()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var query = new SqlCommand("select count(*) from SnapshotStore where SourceId = @SourceId", con);
                query.Parameters.AddWithValue("@SourceId", _sourceId);
                var eventCount = (int)query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(0));
            }
        }
    }
}