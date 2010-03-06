using System.Linq;
using NUnit.Framework;

namespace UnitTests.Domain
{
    [Specification]
    public class WhenClosingTheProperty : InContextOfTestingTheDomain
    {
        protected override void SetupDependencies()
        {
            _subjectUnderTest = Property.New();
            _subjectUnderTest.FlushEvents();
            base.SetupDependencies();
        }

        protected override void When()
        {
            _subjectUnderTest.Close();
        }

        [Then]
        public void ItShouldRaiseThePropertyClosedEventEvent()
        {
            var events = _subjectUnderTest.FlushEvents();
            Assert.That(events.Count(), Is.EqualTo(1));
            Assert.That(events.First().GetType(),Is.EqualTo(typeof(PropertyClosedEvent)));
            Assert.That(_subjectUnderTest.GetEventMonitor().Version, Is.EqualTo(2));
        }
    }
}