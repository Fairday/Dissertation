using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Algorithms.Metrica;
using Dissertation.Algorithms.Algorithms.Model;
using Dissertation.Algorithms.Algorithms.Newton;
using Dissertation.Algorithms.Metrica.MetricaResults;
using Dissertation.Algorithms.Model;
using Dissertation.Algorithms.OrbitalMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Algorithms.Metrica
{
    public class SattelitePeriodicityViewMetrica : MetricaBase<DoubleFeaturePrecedent, SatelliteOld>
    {
        private HashSet<string> _AverageEarchRadiusMRsKeys;
        private HashSet<string> _AverageOrbitHeightMRsKeys;
        private HashSet<string> _AoVMRsKeys;
        private HashSet<string> _AWMRsKeys;
        private HashSet<string> _LWsKeys;
        private HashSet<string> _AlphaMRsKeys;
        private HashSet<string> _SMRsKeys;
        private HashSet<string> _PVMRsKeys;

        public List<AverageEarchRadiusMR> AverageEarchRadiusMRs { get; }
        public List<AverageOrbitHeightMR> AverageOrbitHeightMRs { get; }
        public List<AngleOfViewMR> AoVMRs { get; }
        public List<AngleWidthMR> AWMRs { get; }
        public List<LinearWidthMR> LWMRs { get; }
        public List<CaptureZoneMR> AlphaMRs { get; }
        public List<InvariantSectorSizeMR> SMRs { get; }
        public List<PeriodicityViewMR> PVMRs { get; }

        public SattelitePeriodicityViewMetrica(SatelliteOld @object) : base(@object)
        {
            _AverageEarchRadiusMRsKeys = new HashSet<string>();
            _AverageOrbitHeightMRsKeys = new HashSet<string>();
            _AoVMRsKeys = new HashSet<string>();
            _AWMRsKeys = new HashSet<string>();
            _LWsKeys = new HashSet<string>();
            _AlphaMRsKeys = new HashSet<string>();
            _SMRsKeys = new HashSet<string>();
            _PVMRsKeys = new HashSet<string>();

            AverageEarchRadiusMRs = new List<AverageEarchRadiusMR>();
            AverageOrbitHeightMRs = new List<AverageOrbitHeightMR>();
            AoVMRs = new List<AngleOfViewMR>();
            AWMRs = new List<AngleWidthMR>();
            LWMRs = new List<LinearWidthMR>();
            AlphaMRs = new List<CaptureZoneMR>();
            SMRs = new List<InvariantSectorSizeMR>();
            PVMRs = new List<PeriodicityViewMR>();
        }

        private ObservationVariantTimeInfo GetMaxObservationVariantTimeInfo(ObservationVariantTimeInfo[] observationVariantTimeStreamInfos)
        {
            var max = observationVariantTimeStreamInfos[0];

            foreach (var observationVariantTimeInfo in observationVariantTimeStreamInfos)
            {
                if (observationVariantTimeInfo.ViewPeriodicity > max.ViewPeriodicity)
                    max = observationVariantTimeInfo;
            }

            return max;
        }

        protected override void OnNextEvaluate(DoubleFeaturePrecedent[] featureRow)
        {
            try
            {
                var n = (int)featureRow.First(f => f.Codename == "n").Value;
                var m = (int)featureRow.First(f => f.Codename == "m").Value;
                var i = featureRow.First(f => f.Codename == "i").Value;
                var lattitude = featureRow.First(f => f.Codename == "lattitude").Value;

                var l = (double)m / (double)n;

                if (l > 33d / 2d || l < 1)
                    return;

                //Расчет участков инвариантности наблюдений спутника
                var invariantSectors = CalculateParameters(@object, m, n, i, lattitude);
                //Расчет потоков наблюдений спутника
                var observationVariantTimeStreamInfos = invariantSectors.Select(v => new ObservationVariantTimeInfo(v.ObservationVariant, lattitude, null));
                var max = GetMaxObservationVariantTimeInfo(observationVariantTimeStreamInfos.ToArray());
                var metricaResult8 = new PeriodicityViewMR(max.ObservationVariant, max.ViewPeriodicity, lattitude, m, n, i);
                TryToSaveMetricaResult(PVMRs, _PVMRsKeys, metricaResult8);

                Console.WriteLine($"Calculation with {n} {m} {i} {lattitude} was completed");
            }
            catch (Exception e)
            {              
                if (!e.Message.Contains("При текущей конфигурации параметров"))
                {

                }
                var n = (int)featureRow.First(f => f.Codename == "n").Value;
                var m = (int)featureRow.First(f => f.Codename == "m").Value;
                var i = featureRow.First(f => f.Codename == "i").Value;
                var lattitude = featureRow.First(f => f.Codename == "lattitude").Value;

                Console.WriteLine(e.Message);
                Console.WriteLine($"Calculation with {n} {m} {i} {lattitude} was failed");
            }
        }

        private void TryToSaveMetricaResult<TResult>(IList<TResult> metricaResults, HashSet<string> resultKeys, TResult newResult)
            where TResult : MetricaResultBase
        {
            if (!resultKeys.Contains(newResult.GetKey()))
            {
                resultKeys.Add(newResult.GetKey());
                metricaResults.Add(newResult);
            }
        }

        private SatelliteInvariantSectorInfo[] CalculateParameters(SatelliteOld sattelite, int m, int n, double i, double lattitude)
        {
            sattelite.SetOrbitalInfo(new OrbitParameters(m, n, i));

            //1. Расчет средней высоты орбиты с помощью метода Ньютона (создаем объект алгоритма)
            var orbitAverageHeightAlgorithm = new OrbitAverageHeightAlgorithm(new NewtonAlgorithm());
            //2. Используя алгоритм рассчитываем значения высоты и радиуса орбиты, используя входные условия
            var orbitGeometryInfo = orbitAverageHeightAlgorithm.CalculateOrbitAverageHeight(1E-8, new OrbitParameters(m, n, i));
            sattelite.SetOrbitGeometryInfo(orbitGeometryInfo);
            var metricaResult1 = new AverageOrbitHeightMR(i, m, n, orbitGeometryInfo.AverageHeight);
            TryToSaveMetricaResult(AverageOrbitHeightMRs, _AverageOrbitHeightMRsKeys, metricaResult1);
            //3. Средний радиус Земли
            var averageEarthRadius = OM.AverageEarchRadius(i);
            var metricaResult2 = new AverageEarchRadiusMR(i, averageEarthRadius);
            TryToSaveMetricaResult(AverageEarchRadiusMRs, _AverageEarchRadiusMRsKeys, metricaResult2);
            //4. Расчет угла визирования (Тетта)
            var angleOfView = OM.AngleOfView(averageEarthRadius, orbitGeometryInfo.AverageHeight); var angleOfViewGRAD = angleOfView.ToGrad();
            var metricaResult3 = new AngleOfViewMR(angleOfViewGRAD, averageEarthRadius, orbitGeometryInfo.AverageHeight, m, n, i);
            TryToSaveMetricaResult(AoVMRs, _AoVMRsKeys, metricaResult3);
            //5. Расчет угловой ширины (Бетта)
            var angleWidth = OM.AngleWidth(averageEarthRadius, orbitGeometryInfo.AverageHeight, angleOfView); var angleWidthGRAD = angleWidth.ToGrad();
            var metricaResult4 = new AngleWidthMR(angleWidthGRAD, averageEarthRadius, orbitGeometryInfo.AverageHeight, angleOfViewGRAD, m, n, i);
            TryToSaveMetricaResult(AWMRs, _AWMRsKeys, metricaResult4);
            //6. Расчет полосы обзора (Щелевая зона обзора, П)
            var linearWidth = OM.LinearWidth(averageEarthRadius, angleWidth);
            var metricaResult5 = new LinearWidthMR(linearWidth, averageEarthRadius, angleWidthGRAD, m, n, i);
            TryToSaveMetricaResult(LWMRs, _LWsKeys, metricaResult5);
            //7. Определение принципа наблюдения
            var observationPrinciple = OM.DetermineObservationPrinciple(lattitude, i, angleWidthGRAD);
            sattelite.SetObservationInfo(new ObservationInfo(angleOfView, angleWidth, observationPrinciple));
            //8. Определение зоны захвата (Альфа)
            var captureZone = OM.CalculateCaptureZone(observationPrinciple, lattitude, i, angleWidth, m, n);
            if (double.IsNaN(captureZone.Alpha))
                throw new Exception($"При текущей конфигурации параметров значения зон захватов не существуют. {observationPrinciple}");
            var metricaResult6 = new CaptureZoneMR(captureZone.Alpha.ToGrad(), lattitude, angleWidthGRAD, m, n, i);
            TryToSaveMetricaResult(AlphaMRs, _AlphaMRsKeys, metricaResult6);
            if (observationPrinciple == ObservationPrinciple.Lower)
            {
                //9. Вычисление номера нисходящего узла между B1 и B2
                var p = OM.LowerNodeNumberIn(n, m, lattitude, i);
                //10. Максимальное число узлов и в "Н", и в "В"
                var qmax = OM.Qmax(captureZone, m);
                //11. Расчет остатка деления полного захвата на межузловое расстояние
                var u = OM.U(captureZone, qmax, m);
                //12. Расчет Z, Ksi
                var z = OM.Z(captureZone, n, m, lattitude, i);
                var ksi = OM.Ksi(captureZone, n, m, lattitude, i);
                //13. В зависомости от u, ksi выбираем вариант наблюдения
                var variant = OM.FindObservationVariantIndex(m, u, ksi);
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
                var invariantSectors = OM.GetInvariantSectors(lattitude, sattelite, isp);
                sattelite.SetObservationVariants(invariantSectors);
                var metricaResult7 = new InvariantSectorSizeMR(invariantSectors.Max((s) => s.s), captureZone.Alpha.ToGrad(), lattitude, angleWidthGRAD, m, n, i);
                TryToSaveMetricaResult(SMRs, _SMRsKeys, metricaResult7);
                return invariantSectors;
            }
            else
            {
                //9. Максимальное число узлов и в "Н", и в "В"
                var qmax = OM.Qmax(captureZone, m);
                //10. Расчет остатка деления полного захвата на межузловое расстояние
                var u = OM.U(captureZone, qmax, m);
                var isp = new InvariantSectorsParameters()
                {
                    qmax = qmax,
                    u = u,
                };
                //11. Определение участков инвариантности потоков наблюдения
                var invariantSectors = OM.GetInvariantSectors(lattitude, sattelite, isp);
                sattelite.SetObservationVariants(invariantSectors);
                var metricaResult7 = new InvariantSectorSizeMR(invariantSectors.Max((s) => s.s), captureZone.Alpha.ToGrad(), lattitude, angleWidthGRAD, m, n, i);
                TryToSaveMetricaResult(SMRs, _SMRsKeys, metricaResult7);
                return invariantSectors;
            }
        }
    }
}
