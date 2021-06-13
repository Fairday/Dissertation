using Dissertation.DataCollecting.Abstractions;
using Dissertation.DataCollecting.Reporting.Abstractions;
using System.IO;

namespace Dissertation.DataCollecting.Reporting.Core
{
    internal class FileReportGenerator : IReportGenerator
    {
        private readonly string filename;

        public FileReportGenerator(string filename)
        {
            this.filename = filename;
        }

        public void GenerateReport(IDataSnapshot dataSnapshot)
        {
            using (var file = new StreamWriter(filename))
            {
                foreach (var item in dataSnapshot)
                {
                    file.WriteLine(item);
                }
            }
        }
    }
}