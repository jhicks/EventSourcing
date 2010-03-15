using System;
using System.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqlEventStorage.Tests
{
    [Specification]
    public class WhenReplayingEventsFromAVersion : InContextOfReplayingEventsFromTheSqlEventStore
    {
        private int _versionToReplayFrom;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _versionToReplayFrom = new Random().Next(1, _events.Count());
        }

        protected override void When()
        {
            _replayedEvents = _subjectUnderTest.Replay<string>(_streamId, _versionToReplayFrom);
        }

        [Then]
        public void ItShouldReplayEventsAfterTheVersionInclusive()
        {
            Assert.That(_replayedEvents.Count(), Is.EqualTo(_events.Count() - _versionToReplayFrom + 1));
        }

        [Then]
        public void ItShouldReplayEventsInOrder()
        {
            for (var a = 0; a < _replayedEvents.Count(); a++)
            {
                Assert.That(_replayedEvents.ElementAt(a), Is.EqualTo(_events[a + _versionToReplayFrom - 1]));
            }
        }
    }
}