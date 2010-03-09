using System;

namespace EventSourcing.Db4o
{
    public class Db4oEvent<TEvent> where TEvent : class
    {
        private readonly Guid _streamId;
        private readonly TEvent _event;
        private readonly int _sequence;

        public Db4oEvent(Guid streamId, TEvent @event, int sequence)
        {
            _streamId = streamId;
            _event = @event;
            _sequence = sequence;
        }

        public int Sequence { get { return _sequence; } }
        public Guid StreamId { get { return _streamId; } }
        public TEvent Event { get { return _event; } }
    }
}