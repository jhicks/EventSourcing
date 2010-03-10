using System;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenStoringEventsWithoutATransactionStarted : InContextOfTestingTheEventStore
    {
        protected string[] _events;
        protected Guid _streamId;

        protected override void SetupDependencies()
        {
            _streamId = Guid.NewGuid();
            _events = new[] { "1", "2", "3", "4", "5" };
            base.SetupDependencies();
        }

        protected override void When()
        {
            _subjectUnderTest.StoreEvents(_streamId, _events);
        }

        [Then]
        public void ItShouldThrowAnException()
        {
            Assert.That(_caughtException, Is.Not.Null);
            Assert.That(_caughtException, Is.TypeOf<InvalidOperationException>());
        }
    }
}