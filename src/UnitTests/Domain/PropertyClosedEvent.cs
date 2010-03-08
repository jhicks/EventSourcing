using System;
using EventSourcing.Domain;

namespace UnitTests.Domain
{
    public class PropertyClosedEvent : IDomainEvent
    {
        public Guid SourceId { get; set; }
    }
}