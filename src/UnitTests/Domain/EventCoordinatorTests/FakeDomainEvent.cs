using System;
using EventSourcing.Domain;

namespace UnitTests.Domain.EventCoordinatorTests
{
    public class FakeDomainEvent : IDomainEvent
    {
        public Guid SourceId { get; set; }
    }
}