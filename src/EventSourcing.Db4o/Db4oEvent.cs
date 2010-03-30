using System;

namespace EventSourcing.Db4o
{
    public class Db4oEvent<TEvent> where TEvent : class
    {
        private readonly Guid _streamId;
        private readonly TEvent _event;
        private readonly int _sequence;
        private readonly DateTimeOffset _pointInTime;

        public Db4oEvent(Guid streamId, TEvent @event, int sequence, DateTimeOffset pointInTime)
        {
            _streamId = streamId;
            _event = @event;
            _sequence = sequence;
            _pointInTime = pointInTime;
        }

        public int Sequence { get { return _sequence; } }
        public Guid StreamId { get { return _streamId; } }
        public TEvent Event { get { return _event; } }
        public DateTimeOffset PointInTime { get { return _pointInTime; } }
    }
}