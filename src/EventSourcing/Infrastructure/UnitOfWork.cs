using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Transactions;
using EventSourcing.Domain;
using EventSourcing.EventStorage;

namespace EventSourcing.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IEnlistmentNotification
    {
        private readonly Dictionary<Guid, IAggregateRoot> _identityMap;
        private readonly IEventStore _eventStore;
        private readonly IAggregateBuilder _aggregateBuilder;
        private readonly IEventHandlerFactory _eventHandlerFactory;
        private readonly MethodInfo _handlerFactoryResolverType;
        private static readonly Type EventHandlerTypeInfo;
        private static readonly MethodInfo EventHandlerHandleMethodInfo;

        static UnitOfWork()
        {
            EventHandlerTypeInfo = typeof(IEventHandler<>);
            EventHandlerHandleMethodInfo = EventHandlerTypeInfo.GetMethod("Handle");
        }

        public UnitOfWork(IEventStore eventStore, IAggregateBuilder aggregateBuilder, IEventHandlerFactory eventHandlerFactory)
        {
            EnsureTransaction();

            Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);

            _eventStore = eventStore;
            _aggregateBuilder = aggregateBuilder;
            _eventHandlerFactory = eventHandlerFactory;
            _identityMap = new Dictionary<Guid, IAggregateRoot>();

            _handlerFactoryResolverType = _eventHandlerFactory.GetType().GetMethod("ResolveHandlers").GetGenericMethodDefinition();
        }

        private static void EnsureTransaction()
        {
            if(Transaction.Current == null)
            {
                throw new InvalidOperationException("A running transaction is required");
            }
        }

        public TAggregateRoot GetById<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot
        {
            EnsureTransaction();

            if (_identityMap.ContainsKey(id))
            {
                return (TAggregateRoot)_identityMap[id];
            }

            return Load<TAggregateRoot>(id);
        }

        private TAggregateRoot Load<TAggregateRoot>(Guid id) where TAggregateRoot : class, IAggregateRoot
        {
            EnsureTransaction();

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
            EnsureTransaction();
            _identityMap.Add(aggregateRoot.Id, aggregateRoot);
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            _identityMap.Values.ForEach(CommitAggregateRoot);
            _identityMap.Clear();
            enlistment.Done();
        }

        private void CommitAggregateRoot(IAggregateRoot aggregateRoot)
        {
            var eventsToCommit = aggregateRoot.FlushEvents();

            if (eventsToCommit.Count() == 0)
            {
                return;
            }

            _eventStore.StoreEvents(aggregateRoot.Id, eventsToCommit);

            StoreSnapshot(aggregateRoot);
            RaiseEvents(eventsToCommit);
        }

        private void RaiseEvents(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var @event in domainEvents)
            {
                var method = _handlerFactoryResolverType.MakeGenericMethod(@event.GetType());
                var handlers = (IEnumerable)method.Invoke(_eventHandlerFactory, null);

                foreach (var handler in handlers)
                {
                    EventHandlerHandleMethodInfo.Invoke(handler, new[] { @event });
                }
            }
        }

        private void StoreSnapshot(IAggregateRoot aggregateRoot)
        {
            var snapshotProvider = aggregateRoot as ISnapshotProvider;

            if (snapshotProvider == null || aggregateRoot.Version % snapshotProvider.SnapshotInterval != 0)
            {
                return;
            }

            var snapshot = snapshotProvider.Snapshot();
            _eventStore.StoreSnapshot(aggregateRoot.Id, snapshot);
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            _identityMap.Clear();
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }
}