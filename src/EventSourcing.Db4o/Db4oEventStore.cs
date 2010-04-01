using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Db4objects.Db4o.Linq;
using Db4oFramework;
using EventSourcing.EventStorage;

namespace EventSourcing.Db4o
{
    public class Db4oEventStore : IEventStore
    {
        private readonly Func<ISession> _sessionFactory;

        public Db4oEventStore(ISessionFactory sessionFactory)
            : this(() => sessionFactory.HasBoundSession() ? sessionFactory.GetCurrentSession() : sessionFactory.OpenSession())
        {
        }

        public Db4oEventStore(Func<ISession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        private ISession GetSession()
        {
            return _sessionFactory();
        }

        public void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
        {
            var currentSession = GetSession();
            var sequenceGenerator = currentSession.AsQueryable<SequenceGenerator>().SingleOrDefault(x => x.StreamId == streamId) ?? new SequenceGenerator(streamId);

            foreach(var @event in stream)
            {
                currentSession.Store(new Db4oEvent<TEvent>(streamId, @event, sequenceGenerator.NextSequence(), DateTimeOffset.UtcNow));
            }
            
            currentSession.Store(sequenceGenerator);
        }

        public void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class
        {
            var session = GetSession();
            var currentSnapshot = session.AsQueryable<Db4oSnapshot<TSnapshot>>().SingleOrDefault(x => x.Source == sourceId) ?? new Db4oSnapshot<TSnapshot> {Source = sourceId};
            currentSnapshot.Snapshot = snapshot;
            session.Store(currentSnapshot);
        }

        private IEnumerable<TEvent> Query<TEvent>(Expression<Func<Db4oEvent<TEvent>, bool>> query) where TEvent : class
        {
            return GetSession().AsQueryable<Db4oEvent<TEvent>>().Where(query).OrderBy(x => x.Sequence).Select(x => x.Event);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class
        {
            return Query<TEvent>(x => x.StreamId == streamId);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion) where TEvent : class
        {
            return Query<TEvent>(x => x.StreamId == streamId && x.Sequence >=  fromVersion);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion, int toVersion) where TEvent : class
        {
            return Query<TEvent>(x => x.StreamId == streamId && x.Sequence >= fromVersion && x.Sequence <= toVersion);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime) where TEvent : class
        {
            return Query<TEvent>(x => x.StreamId == streamId && x.PointInTime >= fromPointInTime.UtcDateTime);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime, DateTimeOffset toPointInTime) where TEvent : class
        {
            return Query<TEvent>(x => x.StreamId == streamId && x.PointInTime >= fromPointInTime.UtcDateTime && x.PointInTime <= toPointInTime.UtcDateTime);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime, TimeSpan period) where TEvent : class
        {
            var toPointInTime = fromPointInTime + period;
            return Replay<TEvent>(streamId, fromPointInTime, toPointInTime);
        }

        public TSnapshot LoadSnapshot<TSnapshot>(Guid sourceId) where TSnapshot : class
        {
            var session = GetSession();
            var snapshot = session.AsQueryable<Db4oSnapshot<TSnapshot>>().SingleOrDefault(x => x.Source == sourceId);
            return snapshot == null ? null : snapshot.Snapshot;
        }
    }
}