using System;
using System.Collections.Generic;
using EventSourcing.Domain;

namespace EventSourcing.Infrastructure
{
    public interface IUnitOfWork
    {
        TAggregateRoot GetById<TAggregateRoot>(Guid id) where TAggregateRoot : IAggregateRoot;
        void Add<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : IAggregateRoot;
        void Rollback();
        void Commit(Action<IEnumerable<IDomainEvent>> action);
    }
}