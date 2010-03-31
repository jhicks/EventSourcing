using System;

namespace EventSourcing.Db4o.Tests
{
    public abstract class InContextOfReplayingEvents : InContextOfTestingTheEventStore
    {
        protected Guid _streamId;
        protected string[] _events;
        protected string[] _replayedEvents;
        
        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _streamId = Guid.NewGuid();
            _events = new[] { "1", "2", "3", "4", "5" };

            _subjectUnderTest.StoreEvents(_streamId, _events);
        }
    }
}