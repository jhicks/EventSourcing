using System;
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
}