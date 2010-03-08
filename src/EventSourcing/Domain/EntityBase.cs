using System;

namespace EventSourcing.Domain
{
    public abstract class EntityBase : IEntity
    {
        private readonly Guid _id;
        private readonly EventCoordinator _eventCoordinator;

        protected EntityBase(Guid id, EventCoordinator eventCoordinator)
        {
            _id = id;
            _eventCoordinator = eventCoordinator;
        }

        public Guid Id { get { return _id; } }
        protected EventCoordinator EventCoordinator { get { return _eventCoordinator; } }

        protected void Raise<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            _eventCoordinator.Raise(domainEvent);
        }

        protected void RegisterEventHandler<TDomainEvent>(Action<TDomainEvent> action) where TDomainEvent : IDomainEvent
        {
            _eventCoordinator.RegisterEventHandler(_id, action);
        }
    }
}
