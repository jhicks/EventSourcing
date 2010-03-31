using EventSourcing.Domain;

namespace EventSourcing.Infrastructure
{
    public interface IEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
        void Handle(TDomainEvent @event);
    }
}