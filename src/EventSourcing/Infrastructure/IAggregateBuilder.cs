using System.Collections.Generic;
using EventSourcing.Domain;

namespace EventSourcing.Infrastructure
{
    public interface IAggregateBuilder
    {
        TAggregateRoot BuildFromEventStream<TAggregateRoot>(IEnumerable<IDomainEvent> events) where TAggregateRoot : IAggregateRoot;
        TAggregateRoot BuildFromSnapshot<TAggregateRoot>(ISnapshot snapshot, IEnumerable<IDomainEvent> events) where TAggregateRoot : IAggregateRoot;
    }
}