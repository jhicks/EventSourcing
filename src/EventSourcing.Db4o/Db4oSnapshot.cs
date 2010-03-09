using System;

namespace EventSourcing.Db4o
{
    public class Db4oSnapshot<TSnapsnot> where TSnapsnot : class
    {
        public Guid Source { get; set; }
        public TSnapsnot Snapshot { get; set; }
    }
}