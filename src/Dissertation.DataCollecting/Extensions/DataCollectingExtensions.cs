using System;

namespace Dissertation.DataCollecting.Extensions
{
    public static class DataCollectingExtensions
    {
        public static double Collect(this double value, Func<double, string> prepare) 
        {
            GlobalCollector.DC.Collect(prepare(value));
            return value;
        }

        public static string Collect(this string value, Func<string, string> prepare)
        {
            GlobalCollector.DC.Collect(prepare(value));
            return value;
        }

        public static int Collect(this int value, Func<int, string> prepare)
        {
            GlobalCollector.DC.Collect(prepare(value));
            return value;
        }

        public static T Collect<T>(this T value, Func<T, string> prepare)
        {
            GlobalCollector.DC.Collect(prepare(value));
            return value;
        }
    }
}