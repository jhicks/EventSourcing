using System;
using System.Linq;
using NUnit.Framework;

namespace UnitTests.EventStorage.InMemoryEventStoreTests
{
    [Specification]
    public class WhenStoringANewEventStream : InContextOfTestingTheInMemoryEventStore
    {
        protected override void When()
        {
            using(var transaction = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreEvents(StreamId, Events);
                transaction.Commit();
            }
        }

        [Then]
        public void ItShouldStoreAllTheEvents()
        {
            var results = _subjectUnderTest.Replay<string>(StreamId);
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count(), Is.EqualTo(Events.Count));
        }

        [Then]
        public void ItShouldStoreTheEventsInCorrectSequence()
        {
            var events = _subjectUnderTest.Replay<string>(StreamId).ToArray();
            for (var a = 0; a < events.Count(); a++)
            {
                Assert.That(events[a], Is.EqualTo(Events[a]));
            }
        }
    }

    [Specification]
    public class WhenStoringANewEventStreamOutsideTheScopeOfATransaction : InContextOfTestingTheInMemoryEventStore
    {
        protected override void When()
        {
            _subjectUnderTest.StoreEvents(StreamId, Events);
        }

        [Then]
        public void ItShouldThrowInvalidOperationException()
        {
            Assert.That(_caughtException,Is.Not.Null);
            Assert.That(_caughtException.GetType(),Is.EqualTo(typeof(InvalidOperationException)));
        }
    }
}