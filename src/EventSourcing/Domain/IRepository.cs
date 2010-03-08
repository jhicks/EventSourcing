using System;

namespace EventSourcing.Domain
{
    public interface IRepository
    {
        TAggregateRoot GetById<TAggregateRoot>(Guid aggregateId) where TAggregateRoot : class, IAggregateRoot;
        void Add<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : class, IAggregateRoot;
    }
}