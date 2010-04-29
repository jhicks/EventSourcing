using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqliteEventStorage.Tests
{
    [Specification]
    public class WhenStoringASnapshot : InContextOfTestingTheSqliteEventStore
    {
        private Guid _sourceId;
        private string _snapshot;

        protected override void SetupDependencies()
        {
            _sourceId = Guid.NewGuid();
            _snapshot = "this is a snapshot";
            base.SetupDependencies();
        }

        protected override void When()
        {
            _subjectUnderTest.StoreSnapshot(_sourceId,_snapshot);
        }

        [Then]
        public void ItShouldNotThrowAnException()
        {
            Assert.That(_caughtException,Is.Null);
        }

        [Then]
        public void ItShouldStoreTheSnapshotInTheDatabase()
        {
            using (var con = new SQLiteConnection(_connectionString))
            {
                con.Open();
				var query = new SQLiteCommand("select count(*) from SnapshotStore where SourceId = @SourceId", con);
                query.Parameters.AddWithValue("@SourceId", _sourceId);
                var eventCount = (long)query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(1));
            }
        }

        [TearDown]
        public void CleanupDb()
        {
			using (var con = new SQLiteConnection(_connectionString))
            {
                con.Open();
				var cmd = new SQLiteCommand("delete from SnapshotStore where SourceId = @SourceId", con);
                cmd.Parameters.AddWithValue("@SourceId", _sourceId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}