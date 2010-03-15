using System;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    [Specification]
    public class WhenStoringEventsWithoutATransactionStarted : InContextOfTestingTheSqlEventStore
    {
        protected override void When()
        {
            _subjectUnderTest.StoreEvents(Guid.NewGuid(),new [] {"event 1"});
        }

        [Then]
        public void ItShouldThrowAnInvalidOperationException()
        {
            Assert.That(_caughtException, Is.Not.Null);
            Assert.That(_caughtException, Is.TypeOf<InvalidOperationException>());
        }
    }
}