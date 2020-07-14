namespace Dissertation.Algorithms.ResultHandlers
{
    public interface IResultReceiver<TResult, TInfo>
    {
        void PublishResult(TInfo resultHeader, TResult result);
    }

    public class SatelliteResultListener<TResult, TInfo> : IResultReceiver<TResult, TInfo>
    {
        public void PublishResult(TInfo resultHeader, TResult result)
        {
            throw new System.NotImplementedException();
        }
    }
}
