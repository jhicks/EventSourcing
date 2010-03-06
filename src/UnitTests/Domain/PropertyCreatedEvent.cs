using System;
using EventSourcing.Domain;

namespace UnitTests.Domain
{
    public class PropertyCreatedEvent : IDomainEvent
    {
        public Guid SourceId { get; set; }
    }
}