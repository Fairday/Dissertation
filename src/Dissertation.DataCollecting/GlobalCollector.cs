using Dissertation.DataCollecting.Abstractions;
using Dissertation.DataCollecting.Core;

namespace Dissertation.DataCollecting
{
    public static class GlobalCollector
    {
        public static IDataCollector DataCollector { get; }

        static GlobalCollector() 
        {
            DataCollector = new DataCollector();
        }
    }
}