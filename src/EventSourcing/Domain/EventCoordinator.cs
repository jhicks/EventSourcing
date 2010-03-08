using System;
using System.Collections.Generic;

namespace EventSourcing.Domain
{
    public class EventCoordinator
    {
        private readonly Guid _aggregateRootId;
        private readonly Dictionary<Guid, EventHandlers> _eventHandlers;
        private readonly List<IDomainEvent> _transientEvents;

        public EventCoordinator(Guid aggregateRootId)
        {
            _aggregateRootId = aggregateRootId;
            _transientEvents = new List<IDomainEvent>();
            _eventHandlers = new Dictionary<Guid, EventHandlers>();
        }

        public Guid AggregateRootId { get { return _aggregateRootId; } }
        public int Version { get; private set; }

        public void RegisterEventHandler<TDomainEvent>(Guid sourceId, Action<TDomainEvent> applyAction) where TDomainEvent : IDomainEvent
        {
            EventHandlers eventHandlers;
            if(!_eventHandlers.TryGetValue(sourceId, out eventHandlers))
            {
                eventHandlers = new EventHandlers();
                _eventHandlers.Add(sourceId,eventHandlers);
            }

            eventHandlers.RegisterEventHandler(applyAction);
        }

        public void Raise<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            _transientEvents.Add(domainEvent);
            Dispatch(domainEvent);
        }

        private void Dispatch<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            if(!_eventHandlers.ContainsKey(domainEvent.SourceId))
            {
                throw new Exception("No event handlers registered for the source");
            }

            _eventHandlers[domainEvent.SourceId].Dispatch(domainEvent);
            Version++;
        }

        public IEnumerable<IDomainEvent> Flush()
        {
            var events = _transientEvents.ToArray();
            _transientEvents.Clear();
            return events;
        }

        public void Initialize(IEnumerable<IDomainEvent> eventStream)
        {
            eventStream.ForEach(Dispatch);
        }

        private class EventHandlers
        {
            private readonly Dictionary<Type, Action<IDomainEvent>> _eventHandlers;

            public EventHandlers()
            {
                _eventHandlers = new Dictionary<Type, Action<IDomainEvent>>();
            }

            public void RegisterEventHandler<TDomainEvent>(Action<TDomainEvent> eventHandler) where TDomainEvent : IDomainEvent
            {
                var eventType = typeof(TDomainEvent);
                if (_eventHandlers.ContainsKey(eventType))
                {
                    throw new InvalidOperationException(String.Format("Event handler is already registered for the event {0}", eventType.FullName));
                }

                _eventHandlers.Add(eventType, WrapEventHandler(eventHandler));
            }

            private static Action<IDomainEvent> WrapEventHandler<TDomainEvent>(Action<TDomainEvent> eventHandler)
            {
                return @event => eventHandler((TDomainEvent)@event);
            }

            public void Dispatch<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
            {
                var eventType = domainEvent.GetType();
                if (!_eventHandlers.ContainsKey(eventType))
                {
                    throw new Exception(String.Format("No event handler is already registered for the event {0}", eventType.FullName));
                }

                _eventHandlers[eventType](domainEvent);
            }
        }
    }
}