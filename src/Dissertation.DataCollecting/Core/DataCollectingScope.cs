using Dissertation.DataCollecting.Abstractions;

namespace Dissertation.DataCollecting.Core
{
    internal class DataCollectingScope : IDataCollectingScope
    {
        private readonly DataCollector _dataCollector;
        private bool _isDisposable;

        public DataCollectingScope(DataCollector dataCollector, IDataSnapshot dataSnapshot)
        {
            _dataCollector = dataCollector;
            DataSnapshot = dataSnapshot;
        }

        public IDataSnapshot DataSnapshot { get; }

        public void Dispose()
        {
            _isDisposable = true;
            _dataCollector.FlushOpenedScope();
        }
    }
}