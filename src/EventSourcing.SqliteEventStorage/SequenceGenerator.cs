using System;

namespace EventSourcing.SqliteEventStorage
{
    public class SequenceGenerator
    {
        private readonly Guid _streamId;
        private int _sequence;

    	public SequenceGenerator(Guid streamId) : this(streamId, null)
    	{
    	}

    	public SequenceGenerator(Guid streamId, int? sequence)
        {
            _streamId = streamId;
        	_sequence = sequence.GetValueOrDefault();
        }

        public Guid StreamId { get { return _streamId; } }
        public int NextSequence()
        {
            return ++_sequence;
        }
    }
}