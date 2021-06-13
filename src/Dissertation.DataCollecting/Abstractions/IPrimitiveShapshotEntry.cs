namespace Dissertation.DataCollecting.Abstractions
{
    public interface IPrimitiveShapshotEntry : ISnapshotEntry
    {
        string Value { get; }
    }
}