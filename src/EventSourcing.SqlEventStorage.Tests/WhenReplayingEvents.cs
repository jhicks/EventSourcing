using System.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    [Specification]
    public class WhenReplayingEvents : InContextOfReplayingEventsFromTheSqlEventStore
    {
        protected override void When()
        {
            _replayedEvents = _subjectUnderTest.Replay<string>(_streamId);
        }

        [Then]
        public void ItShouldReplayThemInOrder()
        {
            for (var a = 0; a < _events.Length; a++)
            {
                Assert.That(_replayedEvents.ElementAt(a), Is.EqualTo(_events[a]));
            }
        }

        [Then]
        public void ItShouldReplayAllEvents()
        {
            Assert.That(_replayedEvents, Is.Not.Null);
            Assert.That(_replayedEvents.Count(), Is.EqualTo(_events.Length));
        }
    }
}