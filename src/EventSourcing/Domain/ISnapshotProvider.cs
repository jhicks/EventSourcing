namespace EventSourcing.Domain
{
    public interface ISnapshotProvider
    {
        ISnapshot Snapshot();
        int SnapshotInterval { get; }
    }
}