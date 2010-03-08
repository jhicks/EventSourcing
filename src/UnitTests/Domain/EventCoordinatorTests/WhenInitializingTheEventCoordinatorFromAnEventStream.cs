using System;
using System.Collections.Generic;
using EventSourcing.Domain;
using NUnit.Framework;

namespace UnitTests.Domain.EventCoordinatorTests
{
    [Specification]
    public class WhenInitializingTheEventCoordinatorFromAnEventStream : InContextOfTestingTheEventCoordinator
    {
        private List<IDomainEvent> _eventStream;
        private Guid _sourceId;
        private List<IDomainEvent> _dispatchedEvents;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();

            _sourceId = Guid.NewGuid();
            _dispatchedEvents = new List<IDomainEvent>();
            _eventStream = new List<IDomainEvent> { new FakeDomainEvent { SourceId = _sourceId }, new FakeDomainEvent { SourceId = _sourceId } };

            _subjectUnderTest.RegisterEventHandler<FakeDomainEvent>(_sourceId, @event => _dispatchedEvents.Add(@event));
        }

        protected override void When()
        {
            _subjectUnderTest.Initialize(_eventStream);
        }

        [Then]
        public void ItShouldSetTheVersion()
        {
            Assert.That(_subjectUnderTest.Version, Is.EqualTo(2));
        }

        [Then]
        public void ItShouldDispatchEachEventInTheStream()
        {
            Assert.That(_dispatchedEvents.Count, Is.EqualTo(2));
        }
    }
}