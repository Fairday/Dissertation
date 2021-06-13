using Dissertation.DataCollecting.Abstractions;
using Dissertation.DataCollecting.Reporting.Abstractions;

namespace Dissertation.DataCollecting.Reporting.Extensions
{
    public static class SnapshotExtensions
    {
        public static void GenerateReport(this IDataSnapshot dataSnapshot, IReportGenerator reportGenerator) 
        {
            reportGenerator.GenerateReport(dataSnapshot);
        }
    }
}