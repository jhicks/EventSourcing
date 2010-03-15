using System;
using System.Data.SqlClient;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    [Specification]
    public class WhenStoringASnapshot : InContextOfTestingTheSqlEventStore
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
            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId,_snapshot);
                trans.Commit();
            }
        }

        [Then]
        public void ItShouldNotThrowAnException()
        {
            Assert.That(_caughtException,Is.Null);
        }

        [Then]
        public void ItShouldStoreTheSnapshotInTheDatabase()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var query = new SqlCommand("select count(*) from SnapshotStore where SourceId = @SourceId", con);
                query.Parameters.AddWithValue("@SourceId", _sourceId);
                var eventCount = (int)query.ExecuteScalar();
                Assert.That(eventCount, Is.EqualTo(1));
            }
        }

        [TearDown]
        public void CleanupDb()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("delete from SnapshotStore where SourceId = @SourceId", con);
                cmd.Parameters.AddWithValue("@SourceId", _sourceId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}