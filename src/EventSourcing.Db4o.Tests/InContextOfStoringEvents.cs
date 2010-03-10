using System;
using System.Linq;
using Db4objects.Db4o.Linq;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    public abstract class InContextOfStoringEvents : InContextOfTestingTheEventStore
    {
        protected string[] _events;
        protected Guid _streamId;

        protected override void SetupDependencies()
        {
            _streamId = Guid.NewGuid();
            _events = new[] {"1", "2", "3", "4", "5"};
            base.SetupDependencies();
        }

        [Then]
        public void ItShouldStoreTheEventsInTheDatabase()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var events = session.AsQueryable<Db4oEvent<string>>().Where(x => x.StreamId == _streamId);
                Assert.That(events, Is.Not.Null);
                Assert.That(events.Count(), Is.EqualTo(_events.Length));
            }
        }

        [Then]
        public void ItShouldSetTheSequence()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var events = session.AsQueryable<Db4oEvent<string>>().Where(x => x.StreamId == _streamId).OrderBy(x => x.Sequence);
                for (var a = 0; a < events.Count(); a++)
                {
                    Assert.That(events.ElementAt(a).Sequence, Is.EqualTo(a + 1));
                }
            }
        }

        [Then]
        public void ItShouldSetTheEventsAtTheCorrectSequence()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var events = session.AsQueryable<Db4oEvent<string>>().Where(x => x.StreamId == _streamId).OrderBy(x => x.Sequence);
                for (var a = 0; a < events.Count(); a++)
                {
                    Assert.That(events.ElementAt(a).Event, Is.EqualTo(_events[a]));
                }
            }
        }
    }
}