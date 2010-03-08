using System;

namespace EventSourcing.Domain
{
    public interface IEntity
    {
        Guid Id { get; }
    }
}