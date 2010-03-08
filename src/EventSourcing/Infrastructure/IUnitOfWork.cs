using System;
using System.Collections.Generic;
using EventSourcing.Domain;

namespace EventSourcing.Infrastructure
{
    public interface IUnitOfWork
    {
        TAggregateRoot GetById<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot;
        void Add<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : class, IAggregateRoot;
        void Rollback();
        void Commit(Action<IEnumerable<IDomainEvent>> action);
    }
}