using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace UnitTests.EventStorage.InMemoryEventStoreTests
{
    public abstract class InContextOfReplayingEvents : InContextOfTestingTheInMemoryEventStore
    {
        protected int StartIndex;
        protected int EndIndex = 4;
        protected IEnumerable<object> ReplayedEvents;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();

            using(var transaction = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreEvents(StreamId,Events);
                transaction.Commit();
            }
        }

        [Then]
        public void ItShouldReplayEventsInTheCorrectOrder()
        {
            var eventsAsList = ReplayedEvents.ToList();
            for (var a = 0; a < eventsAsList.Count; a++)
            {
                Assert.That(eventsAsList[a], Is.EqualTo(Events[StartIndex + a]));
            }
        }

        protected void ReplayEvents()
        {
            ReplayedEvents = _subjectUnderTest.Replay<object>(StreamId);
        }
    }
}