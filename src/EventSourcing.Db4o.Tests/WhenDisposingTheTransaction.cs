using System;
using EventSourcing.EventStorage;
using NUnit.Framework;
using UnitTests;

namespace EventSourcing.Db4o.Tests
{
    [Specification]
    public class WhenDisposingTheTransaction : InContextOfTestingTheEventStore
    {
        ITransaction _transaction;
        protected override void SetupDependencies()
        {
            base.SetupDependencies();

            _transaction = _subjectUnderTest.BeginTransaction();
        }

        protected override void When()
        {
            _transaction.Dispose();
        }

        [Then]
        public void ItShouldClearTheTransactionFromTheEventStore()
        {
            using(var session = _sessionFactory.OpenSession())
            {
                _sessionFactory.Bind(session);
                using (var anotherTrans = _subjectUnderTest.BeginTransaction())
                {
                    Assert.That(anotherTrans, Is.Not.Null);
                    Assert.That(anotherTrans, Is.Not.SameAs(_transaction));
                }
                _sessionFactory.Unbind();
            }
        }
    }
}