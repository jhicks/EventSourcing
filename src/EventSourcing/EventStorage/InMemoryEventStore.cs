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

        private Transaction _transaction;

        public InMemoryEventStore()
        {
            _db = new Dictionary<Guid, EventStream>();
        }

        public void Store<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
        {
            if(!_db.ContainsKey(streamId))
            {
                _db.Add(streamId, new EventStream());
            }

            if(_transaction == null)
            {
                throw new InvalidOperationException("Transaction is required");
            }

            _transaction.RegisterAction(() => _db[streamId].Store(stream));
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class
        {
            return _db[streamId].Replay<TEvent>();
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