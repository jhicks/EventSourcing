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

        public TAggregateRoot GetById<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot
        {
            if (_identityMap.ContainsKey(id))
            {
                return (TAggregateRoot)_identityMap[id];
            }

            return Load<TAggregateRoot>(id);
        }

        private TAggregateRoot Load<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot
        {
            var isSnapshotable = typeof(ISnapshotProvider).IsAssignableFrom(typeof(TAggregateRoot));
            var snapshot =  isSnapshotable ? _eventStore.LoadSnapshot<ISnapshot>(id) : null;

            TAggregateRoot ar;
            if(snapshot != null)
            {
                var events = _eventStore.Replay<IDomainEvent>(id, snapshot.Version + 1);
                ar = _aggregateBuilder.BuildFromSnapshot<TAggregateRoot>(snapshot, events);
            }
            else
            {
                var events = _eventStore.Replay<IDomainEvent>(id);
                ar = _aggregateBuilder.BuildFromEventStream<TAggregateRoot>(events);
            }

            if(ar != null)
            {
                _identityMap.Add(id,ar);
            }
            return ar;
        }

        public void Add<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : class, IAggregateRoot
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

            _eventStore.StoreEvents(aggregateRoot.Id, eventsToCommit);

            var snapshotProvider = aggregateRoot as ISnapshotProvider;

            if(snapshotProvider != null && eventsToCommit.Count() >= snapshotProvider.SnapshotInterval)
            {
                var snapshot = snapshotProvider.Snapshot();
                _eventStore.StoreSnapshot(aggregateRoot.Id,snapshot);
            }

            action(eventsToCommit);
        }

        public void Rollback()
        {
            _identityMap.Clear();
        }
    }
}