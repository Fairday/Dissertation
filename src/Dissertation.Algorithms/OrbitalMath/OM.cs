using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Algorithms.Model;
using Dissertation.Algorithms.Algorithms.Newton;
using Dissertation.Algorithms.Model;
using Dissertation.Algorithms.Resources;
using ProcessingModule.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Algorithms.OrbitalMath
{
    /// <summary>
    /// Общая орбитальная математика
    /// </summary>
    public static class OM
    {
        /// <summary>
        /// Средний радиус Земли
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double AverageEarchRadius([Grad]double i)
        {
            var Rcp = Constants.Re * (1 - Constants.alpha * Math.Sin(i.ToRad()) * Math.Sin(i.ToRad()) / 2);
            return Rcp;
        }

        /// <summary>
        /// Угол визирования
        /// </summary>
        /// <param name="averageOrbitRadius"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double AngleOfView(double averageEarchRadius, double averageOrbitRadius)
        {
            var angleOfView = Math.Asin(averageEarchRadius / (averageEarchRadius + averageOrbitRadius));
            return angleOfView;
        }

        /// <summary>
        /// Угловая ширина полосы обзора
        /// </summary>
        /// <param name="averageEarchRadius"></param>
        /// <param name="orbitHeight"></param>
        /// <param name="angleOfView"></param>
        /// <returns></returns>
        public static double AngleWidth(double averageEarchRadius, double orbitHeight, [Rad]double angleOfView)
        {
            var angleWidth = Math.Asin((averageEarchRadius + orbitHeight) / (averageEarchRadius) * Math.Sin(angleOfView)) - angleOfView;
            return angleWidth;
        }

        /// <summary>
        /// Линейная ширина полосы обзора
        /// </summary>
        /// <param name="averageEarchRadius"></param>
        /// <param name="angleWidth"></param>
        /// <returns></returns>
        public static double LinearWidth(double averageEarchRadius, [Rad]double angleWidth)
        {
            var linearWidth = 2 * averageEarchRadius * angleWidth;
            return linearWidth;
        }

        /// <summary>
        /// Номер витка, на котором спутник появляется в nodeNumber узле
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static int VolutionNumber(int m, int n, Node node)
        {
            double a = 0;
            int k = 0;
            do
            {
                a = (k * m + node.Number - 1) / (double)n + 1;
                k++;
            }
            while (!a.IsInteger());

            var intA = (int)a;

            if (intA > m)
            {
                var mInclusions = intA % m;
                intA -= m * mInclusions;
            }

            return intA;
        }

        public static int VolutionNumber(int m, int n, int number)
        {
            double a = 0;
            int k = 0;
            do
            {
                a = (k * m + number - 1) / (double)n + 1;
                k++;
            }
            while (!a.IsInteger());

            var intA = (int)a;

            if (intA > m)
            {
                var mInclusions = intA % m;
                intA -= m * mInclusions;
            }

            return intA;
        }

        /// <summary>
        /// Время пролета спутника над узлом nodeNumber
        /// </summary>
        /// <param name="satellite"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static double OverflightNodeTime(this SatelliteOld satellite, Node node, double lattitude, SatelliteSystemTimeShift[] satelliteSystemTimeShifts)
        {
            if (node.NodeType == NodeType.Vertex || node.NodeType == NodeType.Upstream)
            {
                var a = VolutionNumber(satellite.OrbitParameters.m, satellite.OrbitParameters.n, node);
                var tdr = Tdr(satellite.OrbitGeometryInfo.Radius, satellite.OrbitParameters.i);

                if (satelliteSystemTimeShifts != null)
                {
                    var satelliteShift = satelliteSystemTimeShifts.FirstOrDefault(s => s.SatelliteNumber == satellite.Number);
                    var t = (a - 1) * tdr + (satelliteShift?.deltaTkp ?? 0);// (node.Number == satelliteShift?.delataJkp ? satelliteShift.deltaTkp : 0);
                    return t;
                }
                else
                {
                    var t = (a - 1) * tdr;
                    return t;
                }
            }
            else
            {
                var tdr = Tdr(satellite.OrbitGeometryInfo.Radius, satellite.OrbitParameters.i);
                var a = VolutionNumber(satellite.OrbitParameters.m, satellite.OrbitParameters.n, node);
                var tUpstream = (a - 1) * tdr;
                var deltaNodeT = tdr / (2 * Math.PI) * (Math.PI - 2 * Math.Asin(Math.Sin(lattitude.ToRad()) / Math.Sin(satellite.OrbitParameters.i.ToRad())));
                var tDownstream = tUpstream + deltaNodeT;

                if (satelliteSystemTimeShifts != null)
                {
                    var satelliteShift = satelliteSystemTimeShifts.FirstOrDefault(s => s.SatelliteNumber == satellite.Number);
                    var t = tDownstream + (satelliteShift?.deltaTkp ?? 0);// (node.Number == satelliteShift?.delataJkp ? satelliteShift.deltaTkp : 0);
                    return t;
                }
                else
                {
                    return tDownstream;
                }
            }
        }

        /// <summary>
        /// Драконический период орбиты
        /// </summary>
        /// <param name="averageOrbitRadius"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double Tdr(double averageOrbitRadius, [Grad]double i)
        {
            var multiplier1 = 2 * Math.PI * Math.Pow(averageOrbitRadius, 1.5) / Math.Sqrt(Constants.mu);
            var multiplier2 = 1 - 3d / 8d * Constants.J2 * Math.Pow(Constants.Re / averageOrbitRadius, 2) * (7 * Math.Pow(Math.Cos(i.ToRad()), 2) - 1);
            return multiplier1 * multiplier2;
        }

        /// <summary>
        /// Межузловое расстрояние
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double InterNodalDistance(int m)
            => 2 * Math.PI / m;

        /// <summary>
        /// Межвитковое расстояние
        /// </summary>
        /// <param name="n"></param>
        /// <param name="nodeDistance"></param>
        /// <returns></returns>
        public static double SatteliteTwistDistance(int n, double nodeDistance)
            => n * nodeDistance;

        /// <summary>
        /// Определение принципа наблюдения
        /// </summary>
        /// <param name="lattitude"></param>
        /// <param name="i"></param>
        /// <param name="angleWidth"></param>
        /// <returns></returns>
        public static ObservationPrinciple DetermineObservationPrinciple([Grad]double latitude, [Grad]double i, [Grad]double angleWidth)
        {
            var i0 = Math.Min(i, 180 - i);
            var band = angleWidth;
            if (Math.Abs(latitude) < i0 - band)
                return ObservationPrinciple.Lower;
            else if (Math.Abs(latitude) >= i0 - band && Math.Abs(latitude) < i0 + band && Math.Abs(latitude) <= 180 - (i0 + band))
                return ObservationPrinciple.Upper;
            else
                return ObservationPrinciple.None;
        }

        /// <summary>
        /// Определение принципа наблюдения
        /// </summary>
        /// <param name="sattelite"></param>
        /// <param name="lattitude"></param>
        /// <returns></returns>
        public static ObservationPrinciple DetermineObservationPrinciple(this SatelliteOld sattelite, double lattitude)
        {
            var m = sattelite.OrbitParameters.m;
            var n = sattelite.OrbitParameters.n;
            var i = sattelite.OrbitParameters.i;
            //1. Расчет средней высоты орбиты с помощью метода Ньютона (создаем объект алгоритма)
            var orbitAverageHeightAlgorithm = new OrbitAverageHeightAlgorithm(new NewtonAlgorithm());
            //2. Используя алгоритм рассчитываем значения высоты и радиуса орбиты, используя входные условия
            var oribiInfo = orbitAverageHeightAlgorithm.CalculateOrbitAverageHeight(0.0001, new OrbitParameters(m, n, i));
            //3. Средний радиус Земли
            var averageEarthRadius = AverageEarchRadius(i);
            //4. Расчет угла визирования (Тетта)
            var angleOfView = AngleOfView(averageEarthRadius, oribiInfo.AverageHeight); var angleOfViewGRAD = angleOfView.ToGrad();
            //5. Расчет угловой ширины (Бетта)
            var angleWidth = AngleWidth(averageEarthRadius, oribiInfo.AverageHeight, angleOfView); var angleWidthGRAD = angleWidth.ToGrad();
            //6. Расчет полосы обзора (Щелевая зона обзора, П)
            var linearWidth = LinearWidth(averageEarthRadius, angleWidth);
            //7. Определение принципа наблюдения
            var observationPrinciple = DetermineObservationPrinciple(lattitude, i, angleWidthGRAD);
            return observationPrinciple;
        }

        /// <summary>
        /// Определение зоны захвата спутника
        /// </summary>
        /// <param name="observationPrinciple"></param>
        /// <param name="lattitude"></param>
        /// <param name="i"></param>
        /// <param name="angleWidth"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static ICaptureZone CalculateCaptureZone(ObservationPrinciple observationPrinciple, [Grad]double lattitude, [Grad]double i, [Rad]double angleWidth, int m, int n)
        {
            lattitude = lattitude.ToRad();

            switch (observationPrinciple)
            {
                case ObservationPrinciple.Lower:
                    {
                        var i0 = Math.Min(i, 180 - i).ToRad();
                        i = i.ToRad();
                        var summand1 = Math.Asin((Math.Sin(angleWidth) + Math.Cos(i0) * Math.Sin(lattitude)) / (Math.Sin(i0) * Math.Cos(lattitude)));
                        var summand2 = Math.Asin(Math.Tan(lattitude) / Math.Tan(i0));
                        var multiplier1 = Math.Sign(Math.Cos(i)) * (double)n / (double)m;
                        var multiplier2 = Math.Asin((Math.Sin(lattitude) + Math.Cos(i0) * Math.Sin(angleWidth)) / (Math.Sin(i0) * Math.Cos(angleWidth))) - Math.Asin(Math.Sin(lattitude) / Math.Sin(i0));
                        var summand3 = multiplier1 * multiplier2;

                        var alphaRight = summand1 - summand2 - summand3;

                        summand1 = Math.Asin((Math.Sin(angleWidth) - Math.Cos(i0) * Math.Sin(lattitude)) / (Math.Sin(i0) * Math.Cos(lattitude)));
                        summand2 = Math.Asin(Math.Tan(lattitude) / Math.Tan(i0));
                        multiplier1 = Math.Sign(Math.Cos(i)) * (double)n / (double)m;
                        multiplier2 = Math.Asin((-Math.Sin(lattitude) + Math.Cos(i0) * Math.Sin(angleWidth)) / (Math.Sin(i0) * Math.Cos(angleWidth))) + Math.Asin(Math.Sin(lattitude) / Math.Sin(i0));
                        summand3 = multiplier1 * multiplier2;

                        var alphaLeft = summand1 + summand2 - summand3;

                        return new LowerCaptureZone(alphaLeft, alphaRight);
                    }
                case ObservationPrinciple.Upper:
                    {
                        var i0 = Math.Min(i, 180 - i).ToRad();
                        i = i.ToRad();
                        var tanArgument = lattitude - Math.Atan(Math.Sin(angleWidth) * Math.Cos(lattitude) / (Math.Cos(i0) - Math.Sin(angleWidth) * Math.Sin(lattitude)));
                        var arccosArgument = 1 / Math.Tan(i0) * Math.Tan(tanArgument);
                        var mainMultiplier = Math.Acos(arccosArgument) - Math.Sign(Math.Cos(i)) * (double)n / (double)m * Math.Acos((Math.Sin(lattitude) - Math.Sin(angleWidth) * Math.Cos(i0)) / (Math.Cos(angleWidth) * Math.Sin(i0)));
                        var alpha = 2 * mainMultiplier;
                        return new UpperCaptureZone(alpha);
                    }
                default: return null;
            };
        }

        private static double AcosSafe(this double value)
        {
            if (Math.Abs(value) > 1 && Math.Abs(value) - 1 < 1e-12)
                value = Math.Sign(value);
            return Math.Acos(value);
        }

        /// <summary>
        /// Быстрый поиск захвата для верхних широт без учета вращения Земли
        /// </summary>
        /// <param name="lattitude"></param>
        /// <param name="i"></param>
        /// <param name="angleWidth"></param>
        /// <returns></returns>
        public static double CalculateCaptureZone([Rad]double lattitude, [Rad]double i, [Rad]double angleWidth)
        {
            var u = lattitude;
            var B = angleWidth;

            var angleCos = (Math.Sin(B) - Math.Cos(i) * Math.Sin(u)) / (Math.Sin(i) * Math.Cos(u));
            return Math.Acos(angleCos);
        }

        /// <summary>
        /// Расчет угла разворота
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="lattitude"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double SweepAngle(int n, int m, [Grad]double lattitude, [Grad]double i)
        {
            var sweepAngle = Math.PI * (1 - (double)n / (double)m) - 2 * (Math.Asin(Math.Tan(lattitude.ToRad()) / Math.Tan(i.ToRad())) - (double)n / (double)m * Math.Asin(Math.Sin(lattitude.ToRad()) / Math.Sin(i.ToRad())));
            return sweepAngle;
        }

        /// <summary>
        /// Наибольшее целое число, меньшее действительного числа value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int A(double value)
        {
            var res = (int)Math.Ceiling(value);
            return res - 1;
        }

        /// <summary>
        /// Определение нисходящего узла между B1 и B2
        /// </summary>
        /// <returns></returns>
        public static int LowerNodeNumberIn(int n, int m, [Grad]double lattitude, [Grad]double i)
        {
            int p = 0;

            var sweepAngle = SweepAngle(n, m, lattitude, i);
            var nodeDistance = InterNodalDistance(m);

            var remnant = DivRem(sweepAngle, nodeDistance);

            if (remnant != 0)
            {
                p = (int)Math.Truncate(sweepAngle / nodeDistance) + 2;
            }
            else
            {
                p = (int)Math.Truncate(sweepAngle / nodeDistance) + 1;
            }

            return p;
        }

        /// <summary>
        /// Остаток от деления
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double DivRem(double a, double b)
        {
            var intermediateСalculation = a % b;// a / b - (int)(a / b);
            return intermediateСalculation;
        }

        /// <summary>
        /// Максимальное число узлов, попадающих в каждую из зон В или Н
        /// </summary>
        /// <param name="captureZone"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int Qmax(ICaptureZone captureZone, int m)
        {
            var nodeDistance = InterNodalDistance(m);
            var qmax = (int)Math.Truncate(captureZone.Alpha / nodeDistance) + 1;
            return qmax;
        }

        public static int Qmax(double dxNode, ICaptureZone captureZone)
        {
            var qmax = (int)Math.Truncate(captureZone.Alpha / dxNode) + 1;
            return qmax;
        }

        /// <summary>
        /// Остаток от деления полного захвата на межузловое расстояние
        /// </summary>
        /// <param name="captureZone"></param>
        /// <param name="qmax"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double U(ICaptureZone captureZone, int qmax, int m)
        {
            var nodeDistance = InterNodalDistance(m);
            //q = [a/b]
            //r = a - b * q
            var u = captureZone.Alpha - (qmax - 1) * nodeDistance;
            return u;
        }

        public static double U(ICaptureZone captureZone, int qmax, double dxNode)
        {
            //q = [a/b]
            //r = a - b * q
            var u = captureZone.Alpha - (qmax - 1) * dxNode;
            return u;
        }

        /// <summary>
        /// Правое смещение широтных узлов
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="lattitude"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double RightLattitudeNodeShift(int n, int m, [Grad]double lattitude, [Grad]double i)
        {
            var nodeDistance = InterNodalDistance(m);
            var sweepAngle = SweepAngle(n, m, lattitude, i);

            var rightShift = sweepAngle - nodeDistance * Math.Truncate(sweepAngle / nodeDistance);
            return rightShift;
        }

        /// <summary>
        /// Левое смещение широтных узлов
        /// </summary>
        /// <param name="rightShift"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double LeftLattitudeNodeShift(double rightShift, int m)
        {
            var nodeDistance = InterNodalDistance(m);
            var leftShift = nodeDistance - rightShift;
            return leftShift;
        }

        public static int Z(ICaptureZone captureZone, int n, int m, [Grad]double lattitude, [Grad]double i)
        {
            var nodeDistance = InterNodalDistance(m);
            var rightShift = RightLattitudeNodeShift(n, m, lattitude, i);
            var leftShift = LeftLattitudeNodeShift(rightShift, m);
            var z = (int)Math.Truncate((leftShift + captureZone.DeltaAlpha) / nodeDistance);
            return z;
        }

        public static double Ksi(ICaptureZone captureZone, int n, int m, [Grad]double lattitude, [Grad]double i)
        {
            var nodeDistance = InterNodalDistance(m);
            var rightShift = RightLattitudeNodeShift(n, m, lattitude, i);
            var leftShift = LeftLattitudeNodeShift(rightShift, m);
            int z = Z(captureZone, n, m, lattitude, i);
            var ksi = leftShift + captureZone.DeltaAlpha - z * nodeDistance;
            return ksi;
        }

        /// <summary>
        /// Определение варианта наблюдения
        /// </summary>
        /// <param name="m"></param>
        /// <param name="u"></param>
        /// <param name="ksi"></param>
        /// <returns></returns>
        public static int FindObservationVariantIndex(int m, double u, double ksi)
        {
            var nodeDistance = InterNodalDistance(m);
            var nodeDistanceHalf = nodeDistance / 2;

            if (ksi <= nodeDistanceHalf && u < ksi)
            {
                return 1;
            }
            else if (ksi <= nodeDistanceHalf && (ksi <= u && u < nodeDistance - ksi))
            {
                return 2;
            }
            else if (ksi <= nodeDistanceHalf && u >= nodeDistance - ksi)
            {
                return 3;
            }
            else if (ksi > nodeDistanceHalf && u < nodeDistance - ksi)
            {
                return 4;
            }
            else if (ksi > nodeDistanceHalf && (nodeDistance - ksi <= u && u < ksi))
            {
                return 5;
            }
            else if (ksi > nodeDistanceHalf && u >= ksi)
            {
                return 6;
            }
            else
                throw new KeyNotFoundException("Для данный u, Ksi не найдено соответствующего варианта наблюдения");
        }

        /// <summary>
        /// Определение потоков инвариантности наблюдений
        /// </summary>
        /// <param name="variantIndex"></param>
        public static SatelliteInvariantSectorInfo[] GetInvariantSectors([Grad]double lattitude, SatelliteOld sattelite, InvariantSectorsParameters invariantSectorsParameters)
        {
            var m = sattelite.OrbitParameters.m;
            var u = invariantSectorsParameters.u;
            var qmax = invariantSectorsParameters.qmax;
            var z = invariantSectorsParameters.z;
            var ksi = invariantSectorsParameters.ksi;
            var p = invariantSectorsParameters.p;
            var variantIndex = invariantSectorsParameters.variantIndex;

            var nodeDistance = InterNodalDistance(m);
            var sectors = new List<SatelliteInvariantSectorInfo>();

            if (sattelite.ObservationInfo.ObservationPrinciple == ObservationPrinciple.Lower)
            {
                Func<SatelliteInvariantSectorInfo, ObservationVariant> obserVariantInitializer = (sector) => ConvertLowerObservationPrincipleInvariantSectors(sattelite, sector, p);
                switch (variantIndex)
                {
                    case 1:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - ksi - u, qmax - 1, qmax - 1, z, 2 * qmax - 2, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi - u, qmax - 1, qmax - 1, z + 1, 2 * qmax - 2, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 2:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u - ksi, qmax, qmax, z, 2 * qmax, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(ksi, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(nodeDistance - ksi - u, qmax - 1, qmax - 1, z, 2 * qmax - 2, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 3:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u - ksi, qmax, qmax, z, 2 * qmax, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(-nodeDistance + ksi + u, qmax, qmax, z + 1, 2 * qmax, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 4:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - ksi - u, qmax - 1, qmax - 1, z, 2 * qmax - 2, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi - u, qmax - 1, qmax - 1, z + 1, 2 * qmax - 2, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 5:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(nodeDistance - ksi, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(u - nodeDistance + ksi, qmax, qmax, z + 1, 2 * qmax, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(nodeDistance - ksi, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi - u, qmax - 1, qmax - 1, z + 1, 2 * qmax - 2, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 6:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u - ksi, qmax, qmax - 1, z, 2 * qmax, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(u - nodeDistance + ksi, qmax, qmax, z + 1, 2 * qmax, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                }
            }
            else
            {
                Func<SatelliteInvariantSectorInfo, ObservationVariant> obserVariantInitializer = (sector) => ConvertUpperObservationPrincipleInvariantSectors(sattelite, sector, p);

                if (Math.Abs(lattitude) <= 180 - (Math.Min(sattelite.OrbitParameters.i, 180 - sattelite.OrbitParameters.i) + sattelite.ObservationInfo.AngleWidthGrad))
                {
                    var sector1 = new SatelliteInvariantSectorInfo(u, qmax, null, null, null, obserVariantInitializer);
                    sectors.Add(sector1);
                    if (qmax - 1 > 0)
                    {
                        var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax - 1, null, null, null, obserVariantInitializer);
                        sectors.Add(sector2);
                    }
                }
                else
                {
                    var sector = new SatelliteInvariantSectorInfo(nodeDistance, sattelite.OrbitParameters.m, null, null, null, obserVariantInitializer);

                    sectors.Add(sector);
                }
            }

            return sectors.Where(s => s.ObservationVariant.Variant != string.Empty).ToArray();
        }

        public static SatelliteInvariantSectorInfo[] GetInvariantSectors(ObservationPrinciple observationPrinciple, [Grad]double lattitude, [Grad]double i, [Grad]double band, int m,  InvariantSectorsParameters invariantSectorsParameters)
        {
            var tempSatellite = new SatelliteOld();
            tempSatellite.SetObservationInfo(new ObservationInfo(0, 0, observationPrinciple));

            var u = invariantSectorsParameters.u;
            var qmax = invariantSectorsParameters.qmax;
            var z = invariantSectorsParameters.z;
            var ksi = invariantSectorsParameters.ksi;
            var p = invariantSectorsParameters.p;
            var variantIndex = invariantSectorsParameters.variantIndex;

            var nodeDistance = InterNodalDistance(m);
            var sectors = new List<SatelliteInvariantSectorInfo>();

            if (observationPrinciple == ObservationPrinciple.Lower)
            {
                Func<SatelliteInvariantSectorInfo, ObservationVariant> obserVariantInitializer = (sector) => ConvertLowerObservationPrincipleInvariantSectors(tempSatellite, sector, p);
                switch (variantIndex)
                {
                    case 1:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - ksi - u, qmax - 1, qmax - 1, z, 2 * qmax - 2, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi - u, qmax - 1, qmax - 1, z + 1, 2 * qmax - 2, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 2:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u - ksi, qmax, qmax, z, 2 * qmax, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(ksi, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(nodeDistance - ksi - u, qmax - 1, qmax - 1, z, 2 * qmax - 2, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 3:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u - ksi, qmax, qmax, z, 2 * qmax, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(-nodeDistance + ksi + u, qmax, qmax, z + 1, 2 * qmax, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 4:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - ksi - u, qmax - 1, qmax - 1, z, 2 * qmax - 2, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi - u, qmax - 1, qmax - 1, z + 1, 2 * qmax - 2, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 5:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(nodeDistance - ksi, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(u - nodeDistance + ksi, qmax, qmax, z + 1, 2 * qmax, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(nodeDistance - ksi, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(ksi - u, qmax - 1, qmax - 1, z + 1, 2 * qmax - 2, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                    case 6:
                        {
                            var sector1 = new SatelliteInvariantSectorInfo(u - ksi, qmax, qmax - 1, z, 2 * qmax, obserVariantInitializer);
                            var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax, qmax - 1, z, 2 * qmax - 1, obserVariantInitializer);
                            var sector3 = new SatelliteInvariantSectorInfo(u - nodeDistance + ksi, qmax, qmax, z + 1, 2 * qmax, obserVariantInitializer);
                            var sector4 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax - 1, qmax, z + 1, 2 * qmax - 1, obserVariantInitializer);

                            sectors.Add(sector1);
                            sectors.Add(sector2);
                            sectors.Add(sector3);
                            sectors.Add(sector4);
                        }
                        break;
                }
            }
            else
            {
                Func<SatelliteInvariantSectorInfo, ObservationVariant> obserVariantInitializer = (sector) => ConvertUpperObservationPrincipleInvariantSectors(tempSatellite, sector, p);

                if (Math.Abs(lattitude) <= 180 - (Math.Min(i, 180 - i) + band))
                {
                    var sector1 = new SatelliteInvariantSectorInfo(u, qmax, null, null, null, obserVariantInitializer);
                    sectors.Add(sector1);
                    var sector2 = new SatelliteInvariantSectorInfo(nodeDistance - u, qmax - 1, null, null, null, obserVariantInitializer);
                    sectors.Add(sector2);
                }
                else
                {
                    var sector = new SatelliteInvariantSectorInfo(nodeDistance, m, null, null, null, obserVariantInitializer);

                    sectors.Add(sector);
                }
            }

            return sectors.ToArray();
        }

        /// <summary>
        /// Преобразование инварианта наблюдения в строковый вид (Lower)
        /// </summary>
        /// <param name="variantIndex"></param>
        public static ObservationVariant ConvertLowerObservationPrincipleInvariantSectors(SatelliteOld sattelite, SatelliteInvariantSectorInfo invariantSector, int p)
        {
            if (sattelite.ObservationInfo.ObservationPrinciple == ObservationPrinciple.Upper)
                throw new Exception("Данный метод не пригоден для преобразования инварианта наблюдения c приниципом наблюдения для верхних широт");

            string variant = string.Empty;

            var nodes = new List<Node>();

            for (int i = 0; i < invariantSector.d; i++)
            {
                variant += $"B{i + 1} ";
                nodes.Add(new Node(i + 1, NodeType.Upstream, sattelite));
            }

            for (var i = p - invariantSector.q; i <= p - invariantSector.q + invariantSector.f - 1; i++)
            {
                variant += $"H{i} ";
                nodes.Add(new Node((int)i, NodeType.Downstream, sattelite));
            }

            if (variant != string.Empty)
            {
                return new ObservationVariant(nodes.ToArray(), variant.Substring(0, variant.Length - 1));
            }
            else
            {
                return new ObservationVariant(nodes.ToArray(), string.Empty);
            }
        }

        /// <summary>
        /// Преобразование инварианта наблюдения в строковый вид (Upper)
        /// </summary>
        /// <param name="variantIndex"></param>
        public static ObservationVariant ConvertUpperObservationPrincipleInvariantSectors(SatelliteOld sattelite, SatelliteInvariantSectorInfo invariantSector, int p)
        {
            if (sattelite.ObservationInfo.ObservationPrinciple == ObservationPrinciple.Lower)
                throw new Exception("Данный метод не пригоден для преобразования инварианта наблюдения c приниципом наблюдения для нижних широт");

            string variant = string.Empty;

            var nodes = new List<Node>();

            for (int i = 0; i < invariantSector.d; i++)
            {
                variant += $"V{i + 1} ";
                nodes.Add(new Node(i + 1, NodeType.Vertex, sattelite));
            }

            return new ObservationVariant(nodes.ToArray(), variant == string.Empty ? string.Empty : variant.Substring(0, variant.Length - 1));
        }

        /// <summary>
        /// Преобразование узлов в строковый вид
        /// </summary>
        /// <param name="variantIndex"></param>
        public static string ConvertNodesToString(ObservationPrinciple observationPrinciple, IEnumerable<Node> nodes)
        {
            string variant = string.Empty;

            if (observationPrinciple == ObservationPrinciple.Lower)
            {
                foreach (var node in nodes.Where(n => n.NodeType == NodeType.Upstream))
                    variant += $"B{node.Number}({node.Sattelite.Number}) ";

                foreach (var node in nodes.Where(n => n.NodeType == NodeType.Downstream))
                    variant += $"H{node.Number}({node.Sattelite.Number}) ";

                return variant.Substring(0, variant.Length - 1);
            }
            else
            {
                foreach (var node in nodes)
                    variant += $"V{node.Number}({node.Sattelite.Number}) ";

                return variant.Substring(0, variant.Length - 1);
            }
        }

        /// <summary>
        /// Относительное расположение фундаментальных областей Эk
        /// </summary>
        /// <param name="sattelite"></param>
        /// <returns></returns>
        public static double SatteliteRelative1NodeLongitude(this SatelliteOld sattelite)
        {
            return sattelite.NodeLongitude + (double)sattelite.OrbitParameters.n / (double)sattelite.OrbitParameters.m * sattelite.LatitudeArgument;
        }

        /// <summary>
        /// Количество сдвигов области Эk на величину межузлового расстроения для проектирования k области на область спутника Э1
        /// </summary>
        /// <param name="sattelite"></param>
        /// <param name="satteliteRelative1NodeLongitude"></param>
        /// <returns></returns>
        public static int SatteliteRelativeNodeLongitudeShiftCount(this SatelliteOld sattelite, [Grad]double satteliteRelative1NodeLongitude)
        {
            var nodeDistance = InterNodalDistance(sattelite.OrbitParameters.m);
            return (int)Math.Truncate(satteliteRelative1NodeLongitude.ToRad() / nodeDistance);
        }

        /// <summary>
        /// Долготы трасс спутников
        /// </summary>
        /// <param name="sattelite"></param>
        /// <param name="satteliteRelative1NodeLongitude"></param>
        /// <returns></returns>
        public static double SatteliteRelative2NodeLongitude(this SatelliteOld sattelite, [Grad]double satteliteRelative1NodeLongitude)
        {
            var nodeDistance = InterNodalDistance(sattelite.OrbitParameters.m);
            return DivRem(satteliteRelative1NodeLongitude.ToRad(), nodeDistance);
        }

        /// <summary>
        /// Расчет инвариантов потоков наблюдений
        /// </summary>
        /// <param name="sattelite"></param>
        /// <param name="lattitude"></param>
        /// <returns></returns>
        public static SatelliteInvariantSectorInfo[] CalculateVariants(this SatelliteOld sattelite, IProcessLogger processLogger, [Grad]double lattitude)
        {
            var m = sattelite.OrbitParameters.m;
            var n = sattelite.OrbitParameters.n;
            var i = sattelite.OrbitParameters.i;

            processLogger.Log("Подготовка к расчету", LogStatus.Information);
            var nodeDistance = InterNodalDistance(m);
            var sweepAngle = SweepAngle(n, m, lattitude, i);
            var satteliteTwistDistance = SatteliteTwistDistance(n, nodeDistance);
            processLogger.Log($"Межузловое расстояние: {nodeDistance.ToGrad().Round()}{Symbols.Grad} ({nodeDistance.Round()})", LogStatus.Information);
            processLogger.Log($"Угол разворота: {sweepAngle.ToGrad().Round()}{Symbols.Grad} ({sweepAngle.Round()})", LogStatus.Information);
            processLogger.Log($"Межвитковое расстояние: {satteliteTwistDistance.ToGrad().Round()}{Symbols.Grad} ({satteliteTwistDistance.Round()})", LogStatus.Information);
            processLogger.Log($"-----------------------------------------", LogStatus.Information);
            processLogger.Log($"-----------------------------------------", LogStatus.Information);
            processLogger.Log($"-----------------------------------------", LogStatus.Information);

            //1. Расчет средней высоты орбиты с помощью метода Ньютона (создаем объект алгоритма)
            processLogger.Log("1. Расчет средней высоты орбиты с помощью метода Ньютона (создаем объект алгоритма)", LogStatus.Information);
            var orbitAverageHeightAlgorithm = new OrbitAverageHeightAlgorithm(new NewtonAlgorithm());
            //2. Используя алгоритм рассчитываем значения высоты и радиуса орбиты, используя входные условия
            processLogger.Log("2. Используя алгоритм, рассчитываем значения высоты и радиуса орбиты, используя входные условия", LogStatus.Information);
            var orbitGeometryInfo = orbitAverageHeightAlgorithm.CalculateOrbitAverageHeight(1E-8, new OrbitParameters(m, n, i));
            sattelite.SetOrbitGeometryInfo(orbitGeometryInfo);
            processLogger.Log($"Средний радиус орбиты: {Round(orbitGeometryInfo.Radius)} км", LogStatus.Success);
            processLogger.Log($"Высота орбиты: {Round(orbitGeometryInfo.AverageHeight)} км", LogStatus.Success);
            //3. Средний радиус Земли
            var averageEarthRadius = AverageEarchRadius(i);
            processLogger.Log($"Средний радиус Земли: {Round(averageEarthRadius)} км", LogStatus.Success);
            //4. Расчет угла визирования (Тетта)
            var angleOfView = AngleOfView(averageEarthRadius, orbitGeometryInfo.AverageHeight); var angleOfViewGRAD = angleOfView.ToGrad();
            processLogger.Log($"4. Расчет угла визирования {Symbols.Tetta}: {angleOfViewGRAD.Round()}{Symbols.Grad}", LogStatus.Success);
            //5. Расчет угловой ширины (Бетта)
            var angleWidth = 1999d / (2 * averageEarthRadius); //AngleWidth(averageEarthRadius, orbitGeometryInfo.AverageHeight, angleOfView);
            var angleWidthGRAD = angleWidth.ToGrad();
            processLogger.Log($"5. Расчет угловой ширины {Symbols.Betta}: {angleWidthGRAD.Round()}{Symbols.Grad}", LogStatus.Success);
            //6. Расчет полосы обзора (Щелевая зона обзора, П)
            var linearWidth = 1999d;// LinearWidth(averageEarthRadius, angleWidth);
            processLogger.Log($"6. Расчет полосы обзора (Щелевая зона обзора, П): {linearWidth.Round()} км", LogStatus.Success);
            //7. Определение принципа наблюдения
            var observationPrinciple = DetermineObservationPrinciple(lattitude, i, angleWidthGRAD);
            processLogger.Log($"7. Определение принципа наблюдения: {observationPrinciple.ToString()}", LogStatus.Success);
            sattelite.SetObservationInfo(new ObservationInfo(angleOfView, angleWidth, observationPrinciple));
            //8. Определение зоны захвата (Альфа)
            var captureZone = CalculateCaptureZone(observationPrinciple, lattitude, i, angleWidth, m, n);
            processLogger.Log($"8.1 Определение зоны захвата {Symbols.Alpha}: {captureZone.Alpha.Round()}{Symbols.Grad}", LogStatus.Success);
            processLogger.Log($"8.2 Определение зоны захвата {Symbols.Delta}{Symbols.Alpha}: {captureZone.Alpha.Round()}{Symbols.Grad}", LogStatus.Success);

            if (double.IsNaN(captureZone.Alpha))
            {
                processLogger.Log($"Для данных параметров n, m, i значение {Symbols.Alpha} не существует", LogStatus.Error);
                return null;
            }
            if (observationPrinciple == ObservationPrinciple.Lower)
            {
                //9. Вычисление номера нисходящего узла между B1 и B2
                var p = LowerNodeNumberIn(n, m, lattitude, i);
                processLogger.Log($"9. Вычисление номера нисходящего узла между B1 и B2: {p}", LogStatus.Success);
                //10. Максимальное число узлов и в "Н", и в "В"
                var qmax = Qmax(captureZone, m);
                processLogger.Log($"10. Максимальное число узлов и в \"Н\", и в \"В\": {qmax}", LogStatus.Success);
                //11. Расчет остатка деления полного захвата на межузловое расстояние
                var u = U(captureZone, qmax, m);
                processLogger.Log($"11. Расчет остатка деления полного захвата на межузловое расстояние: {u.Round()}", LogStatus.Success);
                //12. Расчет Z, Ksi
                var z = Z(captureZone, n, m, lattitude, i);
                var ksi = Ksi(captureZone, n, m, lattitude, i);
                processLogger.Log($"12.1 Расчет Z: {z}", LogStatus.Success);
                processLogger.Log($"12.2 Расчет ξ: {ksi.Round()}", LogStatus.Success);
                //13. В зависомости от u, ksi выбираем вариант наблюдения
                var variant = FindObservationVariantIndex(m, u, ksi);
                processLogger.Log($"13. В зависомости от u, ξ выбираем вариант наблюдения: {variant}", LogStatus.Success);
                var isp = new InvariantSectorsParameters()
                {
                    variantIndex = variant,
                    p = p,
                    qmax = qmax,
                    u = u,
                    z = z,
                    ksi = ksi,
                };
                //14. Определение участков инвариантности потоков наблюдения
                processLogger.Log($"14. Определение участков инвариантности потоков наблюдения", LogStatus.Information);
                var invariantSectors = GetInvariantSectors(lattitude, sattelite, isp);
                foreach (var invariantSectorInfo in invariantSectors)
                {
                    processLogger.Log($"Параметры участа инвариантности: {invariantSectorInfo.ToString()}", LogStatus.Success);
                    processLogger.Log($"Узлы наблюдения: {invariantSectorInfo.ObservationVariant.Variant}", LogStatus.Success);
                }
                sattelite.SetObservationVariants(invariantSectors);
                return invariantSectors;
            }
            else
            {
                //9. Максимальное число узлов и в "Н", и в "В"
                var qmax = Qmax(captureZone, m);
                processLogger.Log($"9. Максимальное число узлов и в \"Н\", и в \"В\": {qmax}", LogStatus.Success);
                //10. Расчет остатка деления полного захвата на межузловое расстояние
                var u = U(captureZone, qmax, m);
                processLogger.Log($"10. Расчет остатка деления полного захвата на межузловое расстояние: {u.Round()}", LogStatus.Success);
                var isp = new InvariantSectorsParameters()
                {
                    qmax = qmax,
                    u = u,
                };
                //11. Определение участков инвариантности потоков наблюдения
                processLogger.Log($"11. Определение участков инвариантности потоков наблюдения", LogStatus.Information);
                var invariantSectors = GetInvariantSectors(lattitude, sattelite, isp);
                foreach (var invariantSectorInfo in invariantSectors)
                {
                    processLogger.Log($"Параметры участа инвариантности: {invariantSectorInfo.ToString()}", LogStatus.Success);
                    processLogger.Log($"Узлы наблюдения: {invariantSectorInfo.ObservationVariant.Variant}", LogStatus.Success);
                }
                sattelite.SetObservationVariants(invariantSectors);
                return invariantSectors;
            }
        }

        /// <summary>
        /// Функция приведения величин в интервал
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double ReduceValueToInterval(double x, double xmin, double xmax, double ymin, double ymax, double ymaxReducer = 0.0001)
        {
            var xamp = xmax - xmin;
            var yamp = ymax - ymaxReducer - ymin;
            var koef = yamp / xamp;

            var xd = x - xmin;
            var yd = xd * koef;
            var y = yd + ymin;

            return y;


            //if (ymin + ymaxReducer >= ymax) throw new Exception("Значение верхнего интервала должно быть меньше нижнего с учетом xmaxReducer");
            //var dy = ymax - ymin - ymaxReducer;
            //return x - Math.Floor((x - ymin) / dy) * dy;

        }

        public static double Round(this double value)
            => Math.Round(value, 3);
    }
}
