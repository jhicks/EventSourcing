using System;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenLoadingASnapshotThatDoesNotExist : InContextOfTestingTheEventStore
    {
        private object _snapshot;

        protected override void When()
        {
            _snapshot = _subjectUnderTest.LoadSnapshot<object>(Guid.NewGuid());
        }

        [Then]
        public void ItShouldReturnNull()
        {
            Assert.That(_snapshot, Is.Null);
        }
    }
}