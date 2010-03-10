using System;
using System.Collections.Generic;

namespace EventSourcing.Domain
{
    public abstract class AggregateRootBase : EntityBase, IAggregateRoot
    {
        protected AggregateRootBase(Guid id) : this(new EventCoordinator(id))
        {
        }

        protected AggregateRootBase(EventCoordinator eventCoordinator) : base(eventCoordinator.AggregateRootId,eventCoordinator)
        {
        }

        public IEnumerable<IDomainEvent> FlushEvents()
        {
            return EventCoordinator.Flush();
        }

        public int Version
        {
            get { return EventCoordinator.Version; }
        }
    }
}