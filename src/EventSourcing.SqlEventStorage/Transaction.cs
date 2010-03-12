using System;
using EventSourcing.EventStorage;

namespace EventSourcing.SqlEventStorage
{
    public class Transaction : ITransaction
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}