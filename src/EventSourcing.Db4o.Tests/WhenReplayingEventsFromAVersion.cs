using System;
using System.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenReplayingEventsFromAVersion : InContextOfReplayingEvents
    {
        private int _fromVersion;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _fromVersion = new Random().Next(1, _events.Count());
        }

        protected override void When()
        {
            _replayedEvents = _subjectUnderTest.Replay<string>(_streamId, _fromVersion).ToArray();
        }

        [Then]
        public void ItShouldReplayEventsAfterTheVersionInclusive()
        {
            Assert.That(_replayedEvents.Count(), Is.EqualTo(_events.Count() - _fromVersion + 1));
        }

        [Then]
        public void ItShouldReplayEventsInOrder()
        {
            for (var a = 0; a < _replayedEvents.Count(); a++)
            {
                Assert.That(_replayedEvents.ElementAt(a), Is.EqualTo(_events[a + _fromVersion - 1]));
            }
        }
    }
}