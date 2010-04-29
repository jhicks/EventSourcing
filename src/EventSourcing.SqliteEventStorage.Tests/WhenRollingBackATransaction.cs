using System;
using System.Data.SQLite;
using System.Transactions;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqliteEventStorage.Tests
{
    [Specification]
    public class WhenRollingBackATransaction : InContextOfTestingTheSqliteEventStore
    {
    	private TransactionScope _transaction;
        private Guid _streamId;
        private Guid _sourceId;

        protected override void SetupDependencies()
        {
			_transaction = new TransactionScope();

			base.SetupDependencies();

			_streamId = Guid.NewGuid();
            _sourceId = Guid.NewGuid();

            _subjectUnderTest.StoreEvents(_streamId, new[] {"event 1"});
            _subjectUnderTest.StoreSnapshot(_sourceId, "snapshot");
		}

        protected override void When()
        {
            _transaction.Dispose();
        }

    	[Then]
        public void ItShouldNotStoreEventsInTheDatabase()
        {
            using (var con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                var query = new SQLiteCommand("select count(*) from EventStore where StreamId = @StreamId", con);
                query.Parameters.AddWithValue("@StreamId", _streamId);
                var eventCount = (long) query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(0));
            }
        }

        [Then]
        public void ItShouldNotStoreSnapshotInTheDatabase()
        {
            using (var con = new SQLiteConnection(_connectionString))
            {
                con.Open();
                var query = new SQLiteCommand("select count(*) from SnapshotStore where SourceId = @SourceId", con);
                query.Parameters.AddWithValue("@SourceId", _sourceId);
                var eventCount = (long) query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(0));
            }
        }
    }
}