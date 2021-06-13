namespace Dissertation.DataCollecting.Abstractions
{
    public interface IDataCollector
    {
        IDataCollectingScope Open(string name);
        void Collect(string value);
        IDataSnapshot Get(string name);
        void FlushOpenedScope();
    }
}