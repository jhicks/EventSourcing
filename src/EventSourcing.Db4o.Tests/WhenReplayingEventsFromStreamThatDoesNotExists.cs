using System;
using System.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenReplayingEventsFromStreamThatDoesNotExists : InContextOfTestingTheEventStore
    {
        private object[] _events;
        protected override void When()
        {
            _events = _subjectUnderTest.Replay<object>(Guid.NewGuid()).ToArray();
        }

        [Then]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.That(_events.Length,Is.EqualTo(0));
        }
    }
}