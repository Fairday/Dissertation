using System;

namespace Dissertation.DataCollecting.Abstractions
{
    public interface IDataCollectingScope : IDisposable
    {
        IDataSnapshot DataSnapshot { get; }
    }
}