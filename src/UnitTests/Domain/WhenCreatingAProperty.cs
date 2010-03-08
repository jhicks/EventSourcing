using System.Linq;
using NUnit.Framework;

namespace UnitTests.Domain
{
    [Specification]
    public class WhenCreatingAProperty : InContextOfTestingTheDomain
    {
        protected override void When()
        {
            _subjectUnderTest = Property.New();
        }

        [Then]
        public void ItShouldRaisethePropertyCreatedEvent()
        {
            var events = _subjectUnderTest.FlushEvents();
            Assert.That(events.Count(), Is.EqualTo(1));
            Assert.That(_subjectUnderTest.GetEventMonitor().Version, Is.EqualTo(1));
            Assert.That(events.First().GetType(), Is.EqualTo(typeof(PropertyCreatedEvent)));
        }
    }
}