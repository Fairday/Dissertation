using ProcessingModule.Common;

namespace Dissertation.Algorithms.Abstractions
{
    public interface IMeasurementExecutor<TInput, TAlgorithm, TOutput>
    {
        TOutput Execute(IProcessLogger processLogger, TInput input, TAlgorithm algorithm);
    }
}
