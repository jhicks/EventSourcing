using System;

namespace EventSourcing.Domain
{
    public interface IRepository
    {
        TAggregateRoot GetById<TAggregateRoot>(Guid aggregateId) where TAggregateRoot : IAggregateRoot;
        void Add<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : IAggregateRoot;
    }
}