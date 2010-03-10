using System;
using System.Linq;
using Db4objects.Db4o.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenStoringASnapshotForANewSource : InContextOfTestingTheEventStore
    {
        private Guid _sourceId;
        private string _snapshot;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _snapshot = "Snapshot - " + new Random().Next();
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
        public void ItShouldStoreTheSnapshotInTheDatabase()
        {
            using(var session = _sessionFactory.OpenSession())
            {
                var snapshot = session.AsQueryable<Db4oSnapshot<string>>().Single(x => x.Source == _sourceId);
                Assert.That(snapshot.Snapshot, Is.EqualTo(_snapshot));
            }
        }
    }
}