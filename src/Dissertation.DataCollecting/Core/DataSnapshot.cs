using Dissertation.DataCollecting.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Dissertation.DataCollecting.Core
{
    internal class DataSnapshot : IDataSnapshot
    {
        public DataSnapshot(string name)
        {
            Name = name;
            Data = new List<ISnapshotEntry>();
        }

        public ICollection<ISnapshotEntry> Data { get; }

        public string Name { get; }

        public void AddEntry(ISnapshotEntry snapshotEntry)
        {
            if (snapshotEntry == null)
                throw new ArgumentNullException(nameof(snapshotEntry));

            Data.Add(snapshotEntry);
        }

        public IEnumerator<ISnapshotEntry> GetEnumerator()
        {
            foreach (var item in Data)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }
    }
}