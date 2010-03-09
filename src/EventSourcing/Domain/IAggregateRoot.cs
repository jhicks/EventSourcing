using System.Collections.Generic;

namespace EventSourcing.Domain
{
    public interface IAggregateRoot : IEntity
    {
        IEnumerable<IDomainEvent> FlushEvents();
        int Version { get; }
    }
}