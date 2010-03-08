using System;
using EventSourcing.Domain;

namespace UnitTests.Domain.EventCoordinatorTests
{
    public abstract class InContextOfTestingTheEventCoordinator : BaseTestFixture<EventCoordinator>
    {
        protected Guid _aggregateId;
        protected override void SetupDependencies()
        {
            _aggregateId = Guid.NewGuid();
            _subjectUnderTest = new EventCoordinator(_aggregateId);

            base.SetupDependencies();
        }
    }
}