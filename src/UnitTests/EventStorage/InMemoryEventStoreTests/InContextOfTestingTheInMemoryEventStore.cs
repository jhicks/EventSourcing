using System;
using System.Collections.Generic;
using EventSourcing.EventStorage;

namespace UnitTests.EventStorage.InMemoryEventStoreTests
{
    public abstract class InContextOfTestingTheInMemoryEventStore : BaseTestFixture<InMemoryEventStore>
    {
        protected Guid StreamId;
        protected List<string> Events;

        protected override void SetupDependencies()
        {
            StreamId = Guid.NewGuid();

            Events = new List<string> {"1","2","3","4","5"};

            _subjectUnderTest = new InMemoryEventStore();

            base.SetupDependencies();
        }
    }
}