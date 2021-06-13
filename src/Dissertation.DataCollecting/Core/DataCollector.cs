using Dissertation.DataCollecting.Abstractions;
using System;
using System.Collections.Generic;

namespace Dissertation.DataCollecting.Core
{
    internal class DataCollector : IDataCollector
    {
        private Dictionary<string, IDataSnapshot> _dataSnaphots;

        public DataCollector()
        {
            _dataSnaphots = new Dictionary<string, IDataSnapshot>();
        }

        public IDataCollectingScope CurrentScope { get; private set; }

        public IDataSnapshot Get(string name)
        {
            return _dataSnaphots[name];
        }

        public IDataCollectingScope Open(string name)
        {
            if (CurrentScope != null)
                return CurrentScope;

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Snapshot name cannot be null or empty", nameof(name));

            if (_dataSnaphots.ContainsKey(name))
                throw new InvalidOperationException("Snapshot with such name already exists");

            var snapshot = new DataSnapshot(name);
            _dataSnaphots.Add(name, snapshot);

            return CurrentScope = new DataCollectingScope(this, snapshot);
        }

        public void FlushOpenedScope()
        {
            CurrentScope = null;
        }

        public void Collect(string value)
        {
            if (CurrentScope != null)
                CurrentScope.DataSnapshot.AddEntry(new PrimitiveSnapshotEntry(value));
        }
    }
}