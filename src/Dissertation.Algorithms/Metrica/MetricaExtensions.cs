using System.Collections.Generic;

namespace Dissertation.Algorithms.Metrica
{
    public static class MetricaExtensions
    {
        public static TPrecedent[] PreparePrecedents<TPrecedent>(params TPrecedent[] precedents)
        {
            var list = new List<TPrecedent>();
            list.AddRange(precedents);
            return list.ToArray();
        }
    }
}
