using System;
using System.Data.SqlClient;
using EventSourcing.EventStorage;

namespace EventSourcing.SqlEventStorage
{
    public class Transaction : ITransaction
    {
        private readonly Action _done;
        readonly SqlTransaction _transaction;

        public Transaction(SqlConnection connection, Action done)
        {
            _done = done;
            _transaction = connection.BeginTransaction();
        }

        public SqlTransaction SqlTransaction
        {
            get { return _transaction; }
        }

        public SqlConnection SqlConnection
        {
            get { return _transaction.Connection; }
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _done();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }
    }
}