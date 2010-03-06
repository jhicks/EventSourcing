using System.Linq;
using NUnit.Framework;

namespace UnitTests.EventStorage.InMemoryEventStoreTests
{
    [Specification]
    public class WhenReplayingAllEvents : InContextOfReplayingEvents
    {
        protected override void When()
        {
            ReplayEvents();
        }

        [Then]
        public void ItShouldReplayAllEvents()
        {
            Assert.That(ReplayedEvents.ToList().Count, Is.EqualTo(Events.Count));
        }
    }
}