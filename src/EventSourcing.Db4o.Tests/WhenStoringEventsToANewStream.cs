using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenStoringEventsToANewStream : InContextOfStoringEvents
    {
        protected override void When()
        {
            using(var transaction = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreEvents(_streamId, _events);
                transaction.Commit();
            }
        }
    }
}