using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenStoringEventsToANewStream : InContextOfStoringEvents
    {
        protected override void When()
        {
            _subjectUnderTest.StoreEvents(_streamId, _events);
        }
    }
}