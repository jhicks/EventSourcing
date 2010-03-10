using System.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenReplayingEvents : InContextOfReplayingEvents
    {
        protected override void When()
        {
            _replayedEvents = _subjectUnderTest.Replay<string>(_streamId).ToArray();
        }

        [Then]
        public void ItShouldReplayAllEvents()
        {
            Assert.That(_replayedEvents.Count(),Is.EqualTo(_events.Length));
        }

        [Then]
        public void ItShouldReplayEventsInOrder()
        {
            for(var a = 0; a < _replayedEvents.Count(); a++)
            {
                Assert.That(_replayedEvents.ElementAt(a), Is.EqualTo(_events[a]));
            }
        }
    }
}