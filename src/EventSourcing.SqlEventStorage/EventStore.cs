using System;
using System.Collections.Generic;
using EventSourcing.EventStorage;

namespace EventSourcing.SqlEventStorage
{
    public class EventStore : IEventStore
    {
        public void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class
        {
            throw new NotImplementedException();
        }

        public void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion) where TEvent : class
        {
            throw new NotImplementedException();
        }

        public TSnapshot LoadSnapshot<TSnapshot>(Guid sourceId) where TSnapshot : class
        {
            throw new NotImplementedException();
        }

        public ITransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }
    }
}
