using System;

namespace EventSourcing.Db4o
{
    public class SequenceGenerator
    {
        private readonly Guid _streamId;
        private int _sequence;

        public SequenceGenerator(Guid streamId)
        {
            _streamId = streamId;
        }

        public Guid StreamId { get { return _streamId; } }
        public int NextSequence()
        {
            return ++_sequence;
        }
    }
}