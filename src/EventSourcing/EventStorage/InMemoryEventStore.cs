using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing.EventStorage
{
    /// <summary>
    /// Useless implementation of the event store.  
    /// </summary>
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<Guid, EventStream> _db;
        private readonly Dictionary<Guid, object> _snapshots;

        private Transaction _transaction;

        public InMemoryEventStore()
        {
            _db = new Dictionary<Guid, EventStream>();
            _snapshots = new Dictionary<Guid, object>();
        }

        public void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
        {
            if(!_db.ContainsKey(streamId))
            {
                _db.Add(streamId, new EventStream());
            }

            EnsureTransactionInProgress();
            _transaction.RegisterAction(() => _db[streamId].Store(stream));
        }

        private void EnsureTransactionInProgress()
        {
            if(_transaction == null)
            {
                throw new InvalidOperationException("Transaction is required");
            }
        }

        public void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class
        {
            EnsureTransactionInProgress();
            _transaction.RegisterAction(() => _snapshots[sourceId] = snapshot);
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class
        {
            return _db[streamId].Replay<TEvent>();
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion) where TEvent : class
        {
            return _db[streamId].Replay<TEvent>(fromVersion);
        }

        public TSnapshot LoadSnapshot<TSnapshot>(Guid sourceId) where TSnapshot : class
        {
            if (!_snapshots.ContainsKey(sourceId))
            {
                return null;
            }

            return (TSnapshot)_snapshots[sourceId];
        }

        public ITransaction BeginTransaction()
        {
            if(_transaction != null)
            {
                throw new InvalidOperationException("Transaction already in progress.  Nested transactions are not supported by this implementation");
            }

            _transaction = new Transaction();
            return _transaction;
        }

        private class EventStream
        {
            private readonly List<object> _events;

            public EventStream()
            {
                _events = new List<object>();
            }

            public void Store<TEvent>(IEnumerable<TEvent> stream) where TEvent : class
            {
                _events.AddRange(stream.Cast<object>());
            }

            public IEnumerable<TEvent> Replay<TEvent>() where TEvent : class
            {
                return _events.Cast<TEvent>().ToArray();
            }

            public IEnumerable<TEvent> Replay<TEvent>(int fromVersion)
            {
                return _events.Skip(fromVersion - 1).Cast<TEvent>().ToArray();
            }
        }

        private class Transaction : ITransaction
        {
            private readonly List<Action> _actions;
            private bool _committed;

            public Transaction()
            {
                _actions = new List<Action>();
            }

            public void Dispose()
            {
                if(!_committed)
                {
                    Rollback();
                }
            }

            public void RegisterAction(Action action)
            {
                _actions.Add(action);
            }

            public void Commit()
            {
                _actions.ForEach(action => action());
                _actions.Clear();

                _committed = true;
            }

            public void Rollback()
            {
                _actions.Clear();
            }
        }
    }
}