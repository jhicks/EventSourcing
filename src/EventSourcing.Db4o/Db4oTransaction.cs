using Db4oFramework;
using EventSourcing.EventStorage;

namespace EventSourcing.Db4o
{
    public class Db4oTransaction : ITransaction
    {
        private readonly ISession _session;

        public Db4oTransaction(ISession session)
        {
            _session = session;
        }

        public void Dispose()
        {
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