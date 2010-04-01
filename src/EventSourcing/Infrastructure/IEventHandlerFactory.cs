using System.Collections.Generic;
using EventSourcing.Domain;

namespace EventSourcing.Infrastructure
{
    public interface IEventHandlerFactory
    {
        IEnumerable<IEventHandler<TDomainEvent>> ResolveHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent;
    }
}