using System;
using System.Linq;
using Db4objects.Db4o.Linq;
using Db4oFramework;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    public abstract class InContextOfTestingTheEventStore : BaseTestFixture<Db4oEventStore>
    {
        private string _dbFileName;
        protected ISessionFactory _sessionFactory;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _dbFileName = @".\" + Guid.NewGuid() + ".yap";
            _sessionFactory = new HostedServerSessionFactory(new ThreadStaticCurrentSessionContext(), _dbFileName);
            _subjectUnderTest = new Db4oEventStore(_sessionFactory);

            var session = _sessionFactory.OpenSession();
            _sessionFactory.Bind(session);
        }

        protected override void Finally()
        {
            var session = _sessionFactory.Unbind();
            session.Close();
            session.Dispose();
        }

        [TearDown]
        public void CleanupDatabase()
        {
            _sessionFactory.Dispose();
        }
    }

    [Specification]
    public class WhenStoringEventsWithoutATransactionStarted : InContextOfTestingTheEventStore
    {
        protected string[] _events;
        protected Guid _streamId;

        protected override void SetupDependencies()
        {
            _streamId = Guid.NewGuid();
            _events = new[] { "1", "2", "3", "4", "5" };
            base.SetupDependencies();
        }

        protected override void When()
        {
            _subjectUnderTest.StoreEvents(_streamId, _events);
        }

        [Then]
        public void ItShouldThrowAnException()
        {
            Assert.That(_caughtException, Is.Not.Null);
            Assert.That(_caughtException, Is.TypeOf<InvalidOperationException>());
        }
    }

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

    [Specification]
    public class WhenStoringEventsToAnExistingStream : InContextOfStoringEvents
    {
        protected string[] _additionalEvents;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _additionalEvents = new [] {"6","7","8","9","10","11"};
            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreEvents(_streamId, _events);
                trans.Commit();
            }
            _events = _events.Concat(_additionalEvents).ToArray();
        }

        protected override void When()
        {
            _subjectUnderTest.StoreEvents(_streamId,_additionalEvents);
        }
    }

    [Specification]
    public class WhenStoringASnapshotWithoutATransactionStarted : InContextOfTestingTheEventStore
    {
        protected override void When()
        {
            _subjectUnderTest.StoreSnapshot(Guid.NewGuid(), "this should fail");
        }

        [Then]
        public void ItShouldThrowAnException()
        {
            Assert.That(_caughtException, Is.Not.Null);
        }
    }

    [Specification]
    public class WhenStoringASnapshotForANewSource : InContextOfTestingTheEventStore
    {
        private Guid _sourceId;
        private string _snapshot;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _snapshot = "Snapshot - " + new Random().Next();
        }

        protected override void When()
        {
            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId,_snapshot);
                trans.Commit();
            }
        }

        [Then]
        public void ItShouldStoreTheSnapshotInTheDatabase()
        {
            using(var session = _sessionFactory.OpenSession())
            {
                var snapshot = session.AsQueryable<Db4oSnapshot<string>>().Single(x => x.Source == _sourceId);
                Assert.That(snapshot.Snapshot, Is.EqualTo(_snapshot));
            }
        }
    }

    [Specification]
    public class WhenStoringASnapshotForAnExistingSource : InContextOfTestingTheEventStore
    {
        string _originalSnapshot;
        string _newSnapshot;
        Guid _sourceId;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _originalSnapshot = "Snapshot - " + new Random().Next();
            _newSnapshot = "Snapshot - " + new Random().Next();

            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId,_originalSnapshot);
                trans.Commit();
            }
        }

        protected override void When()
        {
            using (var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId, _newSnapshot);
                trans.Commit();
            }
        }

        [Then]
        public void ItShouldRemoveThePreviousSnapshot()
        {
            using(var session = _sessionFactory.OpenSession())
            {
                Assert.That(session.AsQueryable<Db4oSnapshot<string>>().Where(x => x.Source == _sourceId).Count(), Is.EqualTo(1));
            }
        }

        [Then]
        public void ItShouldStoreTheSnapshotInTheDatabase()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                Assert.That(session.AsQueryable<Db4oSnapshot<string>>().Single(x => x.Source == _sourceId).Snapshot, Is.EqualTo(_newSnapshot));
            }
        }
    }

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

            using (var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreEvents(_streamId, _events);
                trans.Commit();
            }
        }
    }

    [Specification]
    public class WhenReplayingEvents : InContextOfReplayingEvents
    {
        protected override void When()
        {
            _replayedEvents = _subjectUnderTest.Replay<string>(_streamId).ToArray();
        }

        [Then]
        public void ItShouldReplayAllEvents()
        {
            Assert.That(_replayedEvents.Count(),Is.EqualTo(_events.Length));
        }

        [Then]
        public void ItShouldReplayEventsInOrder()
        {
            for(var a = 0; a < _replayedEvents.Count(); a++)
            {
                Assert.That(_replayedEvents.ElementAt(a), Is.EqualTo(_events[a]));
            }
        }
    }

    [Specification]
    public class WhenReplayingEventsFromAVersion : InContextOfReplayingEvents
    {
        private int _fromVersion;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _fromVersion = new Random().Next(1, _events.Count());
        }

        protected override void When()
        {
            _replayedEvents = _subjectUnderTest.Replay<string>(_streamId, _fromVersion).ToArray();
        }

        [Then]
        public void ItShouldReplayEventsAfterTheVersionInclusive()
        {
            Assert.That(_replayedEvents.Count(), Is.EqualTo(_events.Count() - _fromVersion + 1));
        }

        [Then]
        public void ItShouldReplayEventsInOrder()
        {
            for (var a = 0; a < _replayedEvents.Count(); a++)
            {
                Assert.That(_replayedEvents.ElementAt(a), Is.EqualTo(_events[a + _fromVersion - 1]));
            }
        }
    }

    [Specification]
    public class WhenReplayingEventsFromStreamThatDoesNotExists : InContextOfTestingTheEventStore
    {
        object[] _events;
        protected override void When()
        {
            _events = _subjectUnderTest.Replay<object>(Guid.NewGuid()).ToArray();
        }

        [Then]
        public void ItShouldReturnAnEmptyList()
        {
            Assert.That(_events.Length,Is.EqualTo(0));
        }
    }

    [Specification]
    public class WhenLoadingASnapshot : InContextOfTestingTheEventStore
    {
        Guid _sourceId;
        string _snapshot;
        string _loadedSnapshot;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            _sourceId = Guid.NewGuid();
            _snapshot = "this is a test";
            using(var trans = _subjectUnderTest.BeginTransaction())
            {
                _subjectUnderTest.StoreSnapshot(_sourceId,_snapshot);
                trans.Commit();
            }
        }

        protected override void When()
        {
            _loadedSnapshot = _subjectUnderTest.LoadSnapshot<string>(_sourceId);
        }

        [Then]
        public void ItShouldLoadTheCorrectSnapsnot()
        {
            Assert.That(_loadedSnapshot,Is.EqualTo(_snapshot));
        }
    }

    [Specification]
    public class WhenLoadingASnapshotThatDoesNotExist : InContextOfTestingTheEventStore
    {
        object _snapshot;

        protected override void When()
        {
            _snapshot = _subjectUnderTest.LoadSnapshot<object>(Guid.NewGuid());
        }

        [Then]
        public void ItShouldReturnNull()
        {
            Assert.That(_snapshot, Is.Null);
        }
    }
}