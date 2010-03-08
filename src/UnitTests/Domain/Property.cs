using System;
using EventSourcing.Domain;

namespace UnitTests.Domain
{
    public class Property : AggregateRootBase
    {
        private bool _closed;

        private Property() : base(Guid.NewGuid())
        {
            WireUpEventHandlers();
            Raise(new PropertyCreatedEvent {SourceId = Id});
        }

        public static Property New()
        {
            return new Property();
        }

        private void WireUpEventHandlers()
        {
            RegisterEventHandler<PropertyCreatedEvent>(OnPropertyCreated);
            RegisterEventHandler<PropertyClosedEvent>(OnPropertyClosed);
        }

        public void Close()
        {
            if(_closed)
            {
                throw new InvalidOperationException("Property already closed");
            }

            Raise(new PropertyClosedEvent{SourceId = Id});
        }

        private void OnPropertyCreated(PropertyCreatedEvent @event)
        {
            _closed = false;
        }

        private void OnPropertyClosed(PropertyClosedEvent @event)
        {
            _closed = true;
        }

        public EventCoordinator GetEventMonitor()
        {
            return EventCoordinator;
        }
    }
}