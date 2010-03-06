using System;
using EventSourcing.Domain;
using NUnit.Framework;

namespace UnitTests.Domain.EventCoordinatorTests
{
    [Specification]
    public class WhenApplyingAnEventWhichHasNoEventHandlerRegisteredForTheSource : InContextOfTestingTheEventCoordinator
    {
        private class AnotherDomainEvent : IDomainEvent
        {
            public Guid SourceId { get; set; }
        }

        private Guid _sourceId;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();

            _subjectUnderTest.RegisterEventHandler<AnotherDomainEvent>(_sourceId, @event => Console.WriteLine("Handler"));
        }

        protected override void When()
        {
            _subjectUnderTest.Raise(new FakeDomainEvent {SourceId = _sourceId });
        }

        [Then]
        public void ItShouldThrowAnException()
        {
            Assert.That(_caughtException, Is.Not.Null);
            Assert.That(_caughtException.GetType(), Is.EqualTo(typeof(Exception)));
        }
    }
}