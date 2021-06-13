using Dissertation.DataCollecting.Abstractions;

namespace Dissertation.DataCollecting.Core
{
    internal class PrimitiveSnapshotEntry : IPrimitiveShapshotEntry
    {
        public PrimitiveSnapshotEntry(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}