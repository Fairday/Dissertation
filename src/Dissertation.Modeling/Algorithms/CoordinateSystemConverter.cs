using Dissertation.Modeling.Helpers;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using System;

namespace Dissertation.Modeling.Algorithms
{
    public static class CoordinateSystemConverter
    {
        /// <summary>
        /// Перевод углового положения (доглота и широта) в географическую систему координат
        /// </summary>
        /// <param name="earchRadius"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="t"></param>
        /// <param name="angleSpeed"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static Vector Calculate(double earchRadius, Angle latitude, Angle longitude, double t, double angleSpeed, double h = 0)
        {
            var x = (earchRadius + h) * Math.Cos(latitude.Rad) * Math.Cos(angleSpeed * t + longitude.Rad);
            var y = (earchRadius + h) * Math.Cos(latitude.Rad) * Math.Sin(angleSpeed * t + longitude.Rad);
            var z = (earchRadius + h) * Math.Sin(latitude.Rad);

            return new Vector(x, y, z);
        }

        /// <summary>
        /// Проекция фазового положения спутника на Земную поверхность
        /// </summary>
        /// <param name="inclination"></param>
        /// <param name="longitudeAscendingNode"></param>
        /// <param name="latitudeArgument"></param>
        /// <returns></returns>
        public static EarchLocation ProjectSatellitePoint(
            Angle inclination, 
            Angle longitudeAscendingNode,
            Angle latitudeArgument)
        {
            var vector = SatelliteLocationToCoordinatesVector(inclination, longitudeAscendingNode, latitudeArgument);
            return EarchLocationByVector(vector);
        }

        /// <summary>
        /// Перевод фазового положения спутника в декартовые координаты
        /// </summary>
        /// <param name="inclination"></param>
        /// <param name="longitudeAscendingNode"></param>
        /// <param name="latitudeArgument"></param>
        /// <param name="orbitRadius"></param>
        /// <returns></returns>
        public static Vector SatelliteLocationToCoordinates(
            Angle inclination,
            Angle longitudeAscendingNode,
            Angle latitudeArgument,
            double orbitRadius)
        {
            var i = inclination.Rad;
            var L = longitudeAscendingNode.Rad;
            var u = latitudeArgument.Rad;

            var x = Math.Cos(L) * Math.Cos(u) - Math.Sin(L) * Math.Sin(u) * Math.Cos(i);
            var y = Math.Sin(L) * Math.Cos(u) + Math.Cos(L) * Math.Sin(u) * Math.Cos(i);
            var z = Math.Sin(u) * Math.Sin(i);

            return new Vector(x * orbitRadius, y * orbitRadius, z * orbitRadius);
        }

        /// <summary>
        /// Перевод фазового положения спутника в радиус вектор
        /// </summary>
        /// <param name="inclination"></param>
        /// <param name="longitudeAscendingNode"></param>
        /// <param name="latitudeArgument"></param>
        /// <returns></returns>
        public static Vector SatelliteLocationToCoordinatesVector(
            Angle inclination,
            Angle longitudeAscendingNode,
            Angle latitudeArgument)
        {
            var i = inclination.Rad;
            var L = longitudeAscendingNode.Rad;
            var u = latitudeArgument.Rad;

            var x = Math.Cos(L) * Math.Cos(u) - Math.Sin(L) * Math.Sin(u) * Math.Cos(i);
            var y = Math.Sin(L) * Math.Cos(u) + Math.Cos(L) * Math.Sin(u) * Math.Cos(i);
            var z = Math.Sin(u) * Math.Sin(i);

            return new Vector(x, y, z);
        }

        /// <summary>
        /// Перевод фазового положения спутника в вектор скорости
        /// </summary>
        /// <param name="inclination"></param>
        /// <param name="longitudeAscendingNode"></param>
        /// <param name="latitudeArgument"></param>
        /// <returns></returns>
        public static Vector SatelliteLocationToSpeedVector(
            Angle inclination,
            Angle longitudeAscendingNode,
            Angle latitudeArgument)
        {
            var i = inclination.Rad;
            var L = longitudeAscendingNode.Rad;
            var u = latitudeArgument.Rad;

            var x = -Math.Cos(L) * Math.Sin(u) - Math.Sin(L) * Math.Cos(u) * Math.Cos(i);
            var y= -Math.Sin(L) *  Math.Sin(u) + Math.Cos(L) * Math.Cos(u) * Math.Cos(i);
            var z  = Math.Cos(u) * Math.Sin(i);

            return new Vector(x, y, z);
        }

        /// <summary>
        /// Проецирование единичного вектора скорости в прямоугольные координаты
        /// </summary>
        /// <param name="inclination"></param>
        /// <param name="longitudeAscendingNode"></param>
        /// <param name="latitudeArgument"></param>
        /// <returns></returns>
        public static EarchLocation ProjectSatelliteSpeed(
            Angle inclination,
            Angle longitudeAscendingNode,
            Angle latitudeArgument)
        {
            var vector = SatelliteLocationToSpeedVector(inclination, longitudeAscendingNode, latitudeArgument);
            return EarchLocationByVector(vector);
        }

        private  static EarchLocation EarchLocationByVector(Vector vector)
        {
            double latitude = 0;
            double longitude = 0;

            if (Math.Abs(vector.Z) >= 1)
            {
                if (vector.Z > 0)
                    latitude = MathConstants.HPI;
                else
                    latitude = -MathConstants.HPI;
                longitude = 0;// double.NaN;
            }
            else
            {
                latitude = Math.Asin(vector.Z);
                if (Math.Abs(latitude) + MathConstants.SlipAngleRad < MathConstants.HPI)
                    longitude = RangeNormalizer.DPI.Normalize(Math.Atan2(vector.Y, vector.X));
                else
                    longitude = 0;// double.NaN;
            }

            return new EarchLocation(new Angle(latitude, true), new Angle(longitude, true));
        }

        /// <summary>
        /// Определение косинуса углового расстояния между точками
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double CosBy(EarchLocation x, EarchLocation y)
        {
            double cosDeltaE = Math.Cos(x.Longitude.Rad) * Math.Cos(y.Longitude.Rad) +
                               Math.Sin(x.Longitude.Rad) * Math.Sin(y.Longitude.Rad);
            return Math.Cos(x.Latitude.Rad) * Math.Cos(y.Latitude.Rad) * cosDeltaE + Math.Sin(x.Latitude.Rad) * Math.Sin(y.Latitude.Rad);
        }

        /// <summary>
        /// Определение косинуса углового расстояния между точками
        /// Точки задаются прямоугольными координатами на сфере единичного радиуса.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double CosBy(Vector x, Vector y)
        {
            return Vector.CosAngleBetween(x, y);
        }
    }
}
