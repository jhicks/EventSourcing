using System;
using Db4oFramework;
using EventSourcing.EventStorage;

namespace EventSourcing.Db4o
{
    public class Db4oTransaction : ITransaction
    {
        private readonly ISession _session;
        private readonly Action _onDispose;

        public Db4oTransaction(ISession session, Action onDispose)
        {
            _session = session;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }

        public void Commit()
        {
            _session.Commit();
        }

        public void Rollback()
        {
            _session.Rollback();
        }
    }
}