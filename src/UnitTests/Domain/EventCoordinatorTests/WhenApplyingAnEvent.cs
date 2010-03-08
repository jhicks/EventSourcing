using System;
using System.Linq;
using EventSourcing.Domain;
using NUnit.Framework;

namespace UnitTests.Domain.EventCoordinatorTests
{
    [Specification]
    public class WhenApplyingAnEvent : InContextOfTestingTheEventCoordinator
    {
        private Guid _sourceId;
        private IDomainEvent _expectedEvent;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _subjectUnderTest.RegisterEventHandler<FakeDomainEvent>(_sourceId,@event => _expectedEvent = @event);
        }

        protected override void When()
        {
            _subjectUnderTest.Raise(new FakeDomainEvent{SourceId = _sourceId});
        }

        [Then]
        public void ItShouldCallTheRegisteredEventHandler()
        {
            Assert.That(_expectedEvent, Is.Not.Null);
        }

        [Then]
        public void ItShouldIncrementTheVersionNumberByOne()
        {
            Assert.That(_subjectUnderTest.Version, Is.EqualTo(1));
        }

        [Then]
        public void ItShouldTrackTheEvent()
        {
            Assert.That(_subjectUnderTest.Flush().Contains(_expectedEvent), Is.True);
        }
    }
}