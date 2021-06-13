using Dissertation.DataCollecting.Abstractions;
using Dissertation.DataCollecting.Core;

namespace Dissertation.DataCollecting
{
    public static class GlobalCollector
    {
        public static IDataCollector DC { get; }

        static GlobalCollector() 
        {
            DC = new DataCollector();
        }
    }
}