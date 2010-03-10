using System;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenLoadingASnapshot : InContextOfTestingTheEventStore
    {
        private Guid _sourceId;
        private string _snapshot;
        private string _loadedSnapshot;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _snapshot = "this is a test";
            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId,_snapshot);
                trans.Commit();
            }
        }

        protected override void When()
        {
            _loadedSnapshot = _subjectUnderTest.LoadSnapshot<string>(_sourceId);
        }

        [Then]
        public void ItShouldLoadTheCorrectSnapsnot()
        {
            Assert.That(_loadedSnapshot,Is.EqualTo(_snapshot));
        }
    }
}