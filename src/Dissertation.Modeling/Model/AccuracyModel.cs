using Dissertation.Modeling.Model.Basics;
using System;

namespace Dissertation.Modeling.Model
{
    public static class AccuracyModel
    {
        /// <summary>
        /// Угловая ошибка (40 метров для экватора = 6731 * Math.PI * 2 / 1000 = 40 метров)
        /// </summary>
        public const double AngleAccuracyRad = Math.PI * 2 / 1000000;
        public const double AngleAccuracyGrad = AngleAccuracyRad * 180 / Math.PI;
        public const double CalculationAccuracy = 1e-10;
        public const double WeakCalculationAccuracy = 1e-6;
        /// <summary>
        /// Ошибка в метрах
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="earchRadius"></param>
        /// <returns></returns>
        public static double AccuracyByLatitude(Angle latitude, double earchRadius, out double latitudeLength)
        {
            var latitudeRadius = earchRadius * Math.Cos(latitude.Rad);
            latitudeLength = latitudeRadius * 2 * Math.PI;
            return latitudeLength / 1000;
        }
    }
}
