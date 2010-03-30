using System;
using System.Collections.Generic;

namespace EventSourcing.EventStorage
{
    public interface IEventStore
    {
        void StoreEvents<TEvent>(Guid streamId, IEnumerable<TEvent> stream) where TEvent : class;
        IEnumerable<TEvent> Replay<TEvent>(Guid streamId) where TEvent : class;
        IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion) where TEvent : class;
        IEnumerable<TEvent> Replay<TEvent>(Guid streamId, int fromVersion, int toVersion) where TEvent : class;
        IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime) where TEvent : class;
        IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime, TimeSpan period) where TEvent : class;
        IEnumerable<TEvent> Replay<TEvent>(Guid streamId, DateTimeOffset fromPointInTime, DateTimeOffset toPointInTime) where TEvent : class;

        void StoreSnapshot<TSnapshot>(Guid sourceId, TSnapshot snapshot) where TSnapshot : class;
        TSnapshot LoadSnapshot<TSnapshot>(Guid sourceId) where TSnapshot : class;

        ITransaction BeginTransaction();
    }
}