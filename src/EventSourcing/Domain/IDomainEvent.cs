using System;

namespace EventSourcing.Domain
{
    public interface IDomainEvent
    {
        Guid SourceId { get; }
    }
}