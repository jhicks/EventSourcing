using System;
using NUnit.Framework;

namespace UnitTests.Domain.EventCoordinatorTests
{
    [Specification]
    public class WhenApplyingAnEventWhichHasASourceNotPreviouslyRegistered : InContextOfTestingTheEventCoordinator
    {
        protected override void When()
        {
            _subjectUnderTest.Raise(new FakeDomainEvent { SourceId = Guid.NewGuid() });
        }

        [Then]
        public void ItShouldThrowAnException()
        {
            Assert.That(_caughtException, Is.Not.Null);
            Assert.That(_caughtException.GetType(), Is.EqualTo(typeof(Exception)));
        }
    }
}