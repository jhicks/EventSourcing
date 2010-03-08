using System;

namespace EventSourcing.Domain
{
    public interface ISnapshot
    {
        Guid SourceId { get; }
        int Version { get; }
    }
}