using System;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenStoringASnapshotWithoutATransactionStarted : InContextOfTestingTheEventStore
    {
        protected override void When()
        {
            _subjectUnderTest.StoreSnapshot(Guid.NewGuid(), "this should fail");
        }

        [Then]
        public void ItShouldThrowAnException()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }
    }
}