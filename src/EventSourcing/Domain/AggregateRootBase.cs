using System;
using System.Collections.Generic;

namespace EventSourcing.Domain
{
    public abstract class AggregateRootBase : EntityBase, IAggregateRoot
    {
        protected AggregateRootBase(Guid id) : this(id,new EventCoordinator(id))
        {
        }

        protected AggregateRootBase(Guid id, EventCoordinator eventCoordinator) : base(id,eventCoordinator)
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