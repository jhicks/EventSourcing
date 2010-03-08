using System;

namespace EventSourcing.EventStorage
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}