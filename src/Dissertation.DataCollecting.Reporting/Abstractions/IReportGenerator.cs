using Dissertation.DataCollecting.Abstractions;

namespace Dissertation.DataCollecting.Reporting.Abstractions
{
    public interface IReportGenerator
    {
        void GenerateReport(IDataSnapshot dataSnapshot);
    }
}