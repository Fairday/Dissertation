using System.Collections.Generic;

namespace Dissertation.DataCollecting.Abstractions
{
    public interface IDataSnapshot : IEnumerable<ISnapshotEntry>
    {
        string Name { get; }
        ICollection<ISnapshotEntry> Data { get; }
        void AddEntry(ISnapshotEntry snapshotEntry);
    }
}