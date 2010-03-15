using System;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    [Specification]
    public class WhenStoringASnapshotWithoutATransactionStarted : InContextOfTestingTheSqlEventStore
    {
        protected override void When()
        {
            _subjectUnderTest.StoreSnapshot(Guid.NewGuid(),"test snapshot that should fail");
        }

        [Then]
        public void ItShouldThrowAnInvalidOperationException()
        {
            Assert.That(_caughtException, Is.Not.Null);
            Assert.That(_caughtException, Is.TypeOf<InvalidOperationException>());
        }
    }
}