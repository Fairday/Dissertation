using Dissertation.DataCollecting.Reporting.Abstractions;
using Dissertation.DataCollecting.Reporting.Core;
using System;

namespace Dissertation.DataCollecting.Reporting
{
    public static class GlobalReporter
    {
        public static IReportGenerator FileReportGenerator(string filename) 
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("Filename cannot be null or empty", nameof(filename));

            return new FileReportGenerator(filename);
        }
    }
}