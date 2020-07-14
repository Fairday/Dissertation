using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Helpers;
using Dissertation.Algorithms.Model;
using Dissertation.Algorithms.OrbitalMath;
using ProcessingModule.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Algorithms.Algorithms.Model
{
    public class SynthesisOrbitalStructureAlgorithms
    {
        public PeriodicityViewTaskParameters PeriodicityViewTaskParameters { get; }

        public SynthesisOrbitalStructureAlgorithms(PeriodicityViewTaskParameters periodicityViewTaskParameters)
        {
            PeriodicityViewTaskParameters = periodicityViewTaskParameters;
        }

        public SatelliteInvariantSectorInfo[] CalculateSatteliteInvariantSectorInfos(IProcessLogger processLogger, SatelliteOld sattelite, Latitude lattitude)
        {
            var invariantSectors = OM.CalculateVariants(sattelite, processLogger, lattitude.Value);
            return invariantSectors;
        }

        public ObservationVariantTimeInfo[] CalculateObservationVariantTimeStreamInfos(Latitude lattitude, ObservationVariantGroup[] observationVariantGroups,
            SatelliteSystemTimeShift[,] satteliteSystemTimeShifts)
        {
            var observationVariantTimeInfos = new List<ObservationVariantTimeInfo>();

            int i = 0;
            foreach (var observationVariant in observationVariantGroups)
            {
                var allRows = satteliteSystemTimeShifts.GetLength(0);
                var shifts = Enumerable.Range(0, allRows).Select(number => satteliteSystemTimeShifts[number, i]).ToArray();
                var observationVariantTimeInfo = new ObservationVariantTimeInfo(observationVariant.UnionVariant, lattitude.Value, shifts);
                observationVariantTimeInfos.Add(observationVariantTimeInfo);
                i++;
            }

            return observationVariantTimeInfos.ToArray();
        }

        public FundamentalSatelliteZone[] CalculateFundamentalZones(IProcessLogger processLogger, SatelliteOld[] sattelites, double lattitude)
        {
            var fundamentalZones = new List<FundamentalSatelliteZone>();
            foreach (var sattelite in sattelites)
            {
                var invariantSectors = OM.CalculateVariants(sattelite, processLogger, lattitude);
                var sr1 = sattelite.SatteliteRelative1NodeLongitude();
                var gk = sattelite.SatteliteRelativeNodeLongitudeShiftCount(sr1);
                var sr2 = sattelite.SatteliteRelative2NodeLongitude(sr1).ToGrad();
                var fz = new FundamentalSatelliteZone(sr1, gk, sr2, sattelite);
                fundamentalZones.Add(fz);
            }
            return fundamentalZones.ToArray();
        }

        public SatelliteSystemInvariantSectorInfo[] CalculateSatteliteSystemInvariantSectorInfos(FundamentalZonesLimits fundamentalZonesLimits)
        {
            var nodeDistance = fundamentalZonesLimits.NodeDistance;
            var orderedLimitValues = fundamentalZonesLimits.Limits.Select(l => l.Value).Concat(nodeDistance.Yield()).OrderBy(v => v).ToArray();

            var sectorsCount = fundamentalZonesLimits.N * fundamentalZonesLimits.Xi;
            var satteliteSystemInvariantSectorInfos = new SatelliteSystemInvariantSectorInfo[sectorsCount];

            for (int i = 1; i <= sectorsCount; i++)
            {
                var invariantSectorSize = orderedLimitValues[i] - orderedLimitValues[i - 1];
                var averagePointLongitude = (orderedLimitValues[i] + orderedLimitValues[i - 1]) / 2;
                satteliteSystemInvariantSectorInfos[i - 1] = new SatelliteSystemInvariantSectorInfo(invariantSectorSize, averagePointLongitude, i, fundamentalZonesLimits);
            }

            return satteliteSystemInvariantSectorInfos;
        }

        public ObservationVariantGroup[] CalculateSatteliteSystemObservationVariants(FundamentalZonesLimits fundamentalZonesLimits, 
            SatelliteSystemInvariantSectorInfo[] satteliteSystemInvariantSectorInfos)
        {
            var observationVariantsGroups = new List<ObservationVariantGroup>();

            for (int i = 0; i < satteliteSystemInvariantSectorInfos.Length; i++)
            {
                var averPointLong = satteliteSystemInvariantSectorInfos[i].AveragePointLongitude;
                var s = satteliteSystemInvariantSectorInfos[i].S;

                var variantsInGroup = new ObservationVariant[fundamentalZonesLimits.N];

                for (int n = 0; n < fundamentalZonesLimits.N; n++)
                {
                    var nodeDistance = OM.InterNodalDistance(fundamentalZonesLimits.Zones[n].BoundedSattelite.OrbitParameters.m);
                    int variantNumber = -1;
                    var limitsToSearch = fundamentalZonesLimits.OrderedLimits.Skip(n * fundamentalZonesLimits.Xi).Take(fundamentalZonesLimits.Xi).ToArray();
                    for (int j = 0; j < limitsToSearch.Length - 1; j++)
                    {
                        var currentLimit = limitsToSearch[j];
                        var nextLimit = limitsToSearch[j + 1];
                        if (currentLimit.Value <= averPointLong && averPointLong <= nextLimit.Value)
                        {
                            variantNumber = j;
                            break;
                        }
                    }

                    if (averPointLong >= limitsToSearch.Last().Value && averPointLong <= nodeDistance)
                    {
                        variantNumber = fundamentalZonesLimits.Xi - 1;
                    }

                    if (variantNumber != -1)
                    {

                        var observationVariantIndex = fundamentalZonesLimits.ObservationVariantsNumeration.Skip(n * fundamentalZonesLimits.Xi).First(v => v.Item2 == variantNumber + 1);
                        ObservationVariant foundedObservationVariant = fundamentalZonesLimits.Zones[observationVariantIndex.Item1 - 1]
                            .BoundedSattelite.InvariantSectors[observationVariantIndex.Item2 - 1].ObservationVariant;
                        variantsInGroup[n] = foundedObservationVariant;
                    }
                    else
                    {
                        variantsInGroup[n] = null;
                    }
                }

                if (variantsInGroup.Count(v => v != null) > 0)
                {
                    var observationGroup = new ObservationVariantGroup(i + 1, fundamentalZonesLimits.Zones.First().BoundedSattelite.ObservationInfo.ObservationPrinciple,
                        variantsInGroup.Where(v => v != null).ToArray(), s);
                    observationVariantsGroups.Add(observationGroup);
                }
            }

            return observationVariantsGroups.ToArray();
        }

        public SatelliteSystemTimeShift[,] CalculateSatteliteSystemTimeShifts(FundamentalSatelliteZone[] fundamentalSatteliteZones, 
            SatelliteSystemInvariantSectorInfo[] satteliteSystemInvariantSectorInfos)
        {
            var satteliteSystemTimeShifts = new SatelliteSystemTimeShift[fundamentalSatteliteZones.Length - 1, satteliteSystemInvariantSectorInfos.Length];

            for (int k = 1; k < fundamentalSatteliteZones.Length; k++)
            {
                for (int p = 0; p < satteliteSystemInvariantSectorInfos.Length; p++)
                {
                    var zone = fundamentalSatteliteZones[k];

                    var satteliteSystemTimeShift = new SatelliteSystemTimeShift(k + 1, p + 1);

                    var jkp = satteliteSystemInvariantSectorInfos[p].AveragePointLongitude < zone.Tetta2.ToRad() ?
                        zone.Gk + 1 : zone.Gk;

                    var ajkp = Ajkp(zone.BoundedSattelite.OrbitParameters.m, zone.BoundedSattelite.OrbitParameters.n, jkp);

                    var tkp = OM.Tdr(zone.BoundedSattelite.OrbitGeometryInfo.Radius, zone.BoundedSattelite.OrbitParameters.i) *
                        (ajkp - zone.BoundedSattelite.LatitudeArgument.ToRad() / (2 * Math.PI));

                    satteliteSystemTimeShift.SetParameters(tkp, ajkp, jkp);

                    satteliteSystemTimeShifts[k - 1, p] = satteliteSystemTimeShift;
                }
            }

            int Ajkp(int m, int n, int jkp)
            {
                double a = 0;
                int k = 0;
                do
                {
                    a = (k * m + jkp) / (double)n;
                    k++;
                }
                while (!a.IsInteger());

                var intA = (int)a;

                if (intA > m - 1)
                {
                    var mInclusions = intA % (m - 1);
                    intA -= (m - 1) * mInclusions;
                }

                return intA;
            }

            return satteliteSystemTimeShifts;
        }
    }
}
