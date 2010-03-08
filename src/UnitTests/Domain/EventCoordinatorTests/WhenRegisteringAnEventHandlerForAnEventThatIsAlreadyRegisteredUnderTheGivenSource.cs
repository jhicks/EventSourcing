using System;
using NUnit.Framework;

namespace UnitTests.Domain.EventCoordinatorTests
{
    [Specification]
    public class WhenRegisteringAnEventHandlerForAnEventThatIsAlreadyRegisteredUnderTheGivenSource : InContextOfTestingTheEventCoordinator
    {
        protected Guid _sourceId;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _subjectUnderTest.RegisterEventHandler<FakeDomainEvent>(_sourceId, @event => Console.WriteLine("Handler"));
        }

        protected override void When()
        {
            _subjectUnderTest.RegisterEventHandler<FakeDomainEvent>(_sourceId, @event => Console.WriteLine("Handler"));
        }

        [Then]
        public void ItShouldThrowAnInvalidOperationException()
        {
            Assert.That(_caughtException, Is.Not.Null);
            Assert.That(_caughtException.GetType(), Is.EqualTo(typeof(InvalidOperationException)));
        }
    }
}