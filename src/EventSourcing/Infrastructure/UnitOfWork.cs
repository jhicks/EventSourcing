using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Domain;
using EventSourcing.EventStorage;

namespace EventSourcing.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Guid, IAggregateRoot> _identityMap;
        private readonly IEventStore _eventStore;
        private readonly IAggregateBuilder _aggregateBuilder;

        public UnitOfWork(IEventStore eventStore, IAggregateBuilder aggregateBuilder)
        {
            _eventStore = eventStore;
            _aggregateBuilder = aggregateBuilder;
            _identityMap = new Dictionary<Guid, IAggregateRoot>();
        }

        public TAggregateRoot GetById<TAggregateRoot>(Guid id) where TAggregateRoot : IAggregateRoot
        {
            if (_identityMap.ContainsKey(id))
            {
                return (TAggregateRoot)_identityMap[id];
            }

            return Load<TAggregateRoot>(id);
        }

        private TAggregateRoot Load<TAggregateRoot>(Guid id) where TAggregateRoot : IAggregateRoot
        {
            var events = _eventStore.Replay<IDomainEvent>(id);
            var ar = _aggregateBuilder.BuildFromEventStream<TAggregateRoot>(events);
            _identityMap.Add(id,ar);
            return ar;
        }

        public void Add<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : IAggregateRoot
        {
            _identityMap.Add(aggregateRoot.Id, aggregateRoot);
        }

        public void Commit(Action<IEnumerable<IDomainEvent>> action)
        {
            using (var transaction = _eventStore.BeginTransaction())
            {
                _identityMap.Values.ForEach(ar => CommitAggregate(ar,action));
                transaction.Commit();
                _identityMap.Clear();
            }
        }

        private void CommitAggregate(IAggregateRoot aggregateRoot, Action<IEnumerable<IDomainEvent>> action)
        {
            var eventsToCommit = aggregateRoot.FlushEvents();

            if (eventsToCommit.Count() == 0)
            {
                return;
            }

            _eventStore.Store(aggregateRoot.Id, eventsToCommit);
            action(eventsToCommit);
        }

        public void Rollback()
        {
            _identityMap.Clear();
        }
    }
}