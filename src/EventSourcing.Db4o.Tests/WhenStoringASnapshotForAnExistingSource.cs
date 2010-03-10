using System;
using System.Linq;
using Db4objects.Db4o.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenStoringASnapshotForAnExistingSource : InContextOfTestingTheEventStore
    {
        private string _originalSnapshot;
        private string _newSnapshot;
        private Guid _sourceId;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _originalSnapshot = "Snapshot - " + new Random().Next();
            _newSnapshot = "Snapshot - " + new Random().Next();

            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId,_originalSnapshot);
                trans.Commit();
            }
        }

        protected override void When()
        {
            using (var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId, _newSnapshot);
                trans.Commit();
            }
        }

        [Then]
        public void ItShouldRemoveThePreviousSnapshot()
        {
            using(var session = _sessionFactory.OpenSession())
            {
                Assert.That(session.AsQueryable<Db4oSnapshot<string>>().Where(x => x.Source == _sourceId).Count(), Is.EqualTo(1));
            }
        }

        [Then]
        public void ItShouldStoreTheSnapshotInTheDatabase()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                Assert.That(session.AsQueryable<Db4oSnapshot<string>>().Single(x => x.Source == _sourceId).Snapshot, Is.EqualTo(_newSnapshot));
            }
        }
    }
}