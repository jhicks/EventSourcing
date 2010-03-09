using System;
using System.Collections.Generic;

namespace EventSourcing.Domain
{
    public abstract class AggregateRootBase : EntityBase, IAggregateRoot
    {
        protected AggregateRootBase(Guid id) : base(id,new EventCoordinator(id))
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