using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.SqliteEventStorage.Tests
{
    [Specification]
    public class WhenReplayingEventsFromAStreamThatDoesNotExist : InContextOfTestingTheSqliteEventStore
    {
        private IEnumerable<string> _replayedEvents;

        protected override void When()
        {
            _replayedEvents = _subjectUnderTest.Replay<string>(Guid.NewGuid());
        }

        [Then]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.That(_replayedEvents, Is.Not.Null);
            Assert.That(_replayedEvents.Count(), Is.EqualTo(0));
        }
    }
}