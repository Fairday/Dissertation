namespace Dissertation.Algorithms.ResultHandlers
{
    public interface IResultProcessor<TResult, TInfo>
    {
        void Process(TInfo resultHeader, TResult result);
    }

    public class GraphicResultProcessor
    {

    }

    public class ViewContainerResultProcessor
    {

    }

    public class StorageResultProcessor
    {

    }
}
