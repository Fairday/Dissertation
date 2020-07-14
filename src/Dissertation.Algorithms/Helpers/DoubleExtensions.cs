using System;

namespace Dissertation.Algorithms.Algorithms.Helpers
{
    public static class DoubleExtensions
    {
        public static double ToRad(this double value) 
            => value * Math.PI / 180;

        public static double ToGrad(this double value)
            => value * 180 / Math.PI;

        /// <summary>
        /// Проверка: является ли число целым
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool IsInteger(this double x)
        {
            double eps = 1E-18;
            if (Math.Abs(x % 1) < eps)
                return true;
            else
                return false;
        }

        public static double Round(this double value)
            => Math.Round(value, 3);
    }
}
