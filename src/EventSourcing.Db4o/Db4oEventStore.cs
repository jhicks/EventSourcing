using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using Db4oFramework;
using EventSourcing.EventStorage;

namespace EventSourcing.Db4o
{
    public class Db4oEventStore : IEventStore
    {
        private readonly ISessionFactory _sessionFactory;
        private Db4oTransaction _transaction;

        public Db4oEventStore(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
        {
            EnsureTransaction();

            var currentSession = _sessionFactory.GetCurrentSession();
            var sequenceGenerator = currentSession.AsQueryable<SequenceGenerator>().SingleOrDefault(x => x.StreamId == streamId) ?? new SequenceGenerator(streamId);

            foreach(var @event in stream)
            {
                currentSession.Store(new Db4oEvent<TEvent>(streamId, @event, sequenceGenerator.NextSequence()));
            }
            
            currentSession.Store(sequenceGenerator);
        }

        private void EnsureTransaction()
        {
            if(_transaction == null)
            {
                throw new InvalidOperationException("Open transaction is required");
            }
        }

        public void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class
        {
            EnsureTransaction();

            var session = _sessionFactory.GetCurrentSession();
            var currentSnapshot = session.AsQueryable<Db4oSnapshot<TSnapshot>>().SingleOrDefault(x => x.Source == sourceId) ?? new Db4oSnapshot<TSnapshot> {Source = sourceId};
            currentSnapshot.Snapshot = snapshot;
            session.Store(currentSnapshot);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class
        {
            var session = _sessionFactory.GetCurrentSession();
            return session.AsQueryable<Db4oEvent<TEvent>>().Where(x => x.StreamId == streamId).OrderBy(x => x.Sequence).Select(x => x.Event);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion) where TEvent : class
        {
            var session = _sessionFactory.GetCurrentSession();
            return session.AsQueryable<Db4oEvent<TEvent>>().Where(x => x.StreamId == streamId && x.Sequence >= fromVersion).OrderBy(x => x.Sequence).Select(x => x.Event);
        }

        public TSnapshot LoadSnapshot<TSnapshot>(Guid sourceId) where TSnapshot : class
        {
            var session = _sessionFactory.GetCurrentSession();
            var snapshot = session.AsQueryable<Db4oSnapshot<TSnapshot>>().SingleOrDefault(x => x.Source == sourceId);
            return snapshot == null ? null : snapshot.Snapshot;
        }

        public ITransaction BeginTransaction()
        {
            if(_transaction != null)
            {
                throw new Exception("Transaction already in progress");
            }

            _transaction = new Db4oTransaction(_sessionFactory.GetCurrentSession());
            return _transaction;
        }
    }
}