using System;
using System.Collections.Generic;

namespace EventSourcing.EventStorage
{
    public interface IEventStore
    {
        void Store<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class;
        IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class;
        ITransaction BeginTransaction();
    }
}