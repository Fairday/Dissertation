using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.OrbitalMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Algorithms.Model
{
    public class FundamentalZonesLimits
    {
        public double NodeDistance { get; private set; }
        public FundamentalSatelliteZone[] Zones { get; }

        public FundamentalZonesLimits(FundamentalSatelliteZone[] fundamentalSatteliteZones)
        {
            Zones = fundamentalSatteliteZones;
            CalculateLimits(Zones);
        }

        public Limit[,] LimitsMatrix { get; private set; }
        public Limit[] Limits { get; private set; }
        public Limit[] OrderedLimits { get; private set; }
        public Tuple<int, int>[] ObservationVariantsNumeration { get; private set; }
        public int N { get; private set; }
        public int Xi { get; private set; }

        internal void CalculateLimits(FundamentalSatelliteZone[] zones)
        {
            NodeDistance = OM.InterNodalDistance(zones[0].BoundedSattelite.OrbitParameters.m);
            N = zones.Length;
            Xi = zones[0].BoundedSattelite.InvariantSectors.Length;
            LimitsMatrix = new Limit[N, Xi];
            var limits = new List<Limit>();
            LimitsMatrix[0, 0] = new Limit(1, 1, 0);
            limits.Add(LimitsMatrix[0, 0]);

            for (int j = 1; j < Xi; j++)
            {
                var newLimitValue = LimitsMatrix[0, j - 1].Value + zones[0].BoundedSattelite.InvariantSectors[j - 1].s;
                LimitsMatrix[0, j] = new Limit(1, j + 1, newLimitValue);
                limits.Add(LimitsMatrix[0, j]);
            }

            for (int k = 2; k <= N; k++)
            {
                var tetta2 = zones[k - 1].Tetta2.ToRad();
                LimitsMatrix[k - 1, 0] = new Limit(k, 1, tetta2);
                limits.Add(LimitsMatrix[k - 1, 0]);

                for (int j = 1; j < Xi; j++)
                {
                    var newLimitValue = LimitsMatrix[k - 1, j - 1].Value + zones[k - 1].BoundedSattelite.InvariantSectors[j - 1].s;
                    var reducedValue = j == 1 ? OM.ReduceValueToInterval(newLimitValue, tetta2, NodeDistance + tetta2, 0, NodeDistance) : newLimitValue;
                    LimitsMatrix[k - 1, j] = new Limit(k, j + 1, reducedValue);
                    limits.Add(LimitsMatrix[k - 1, j]);
                }
            }

            Limits = limits.ToArray();

            OrderedLimits = Limits.OrderBy(l => l.Index1).ThenBy(l => l.Value).ThenBy(l => l.Index2).ToArray();
            ObservationVariantsNumeration = Limits.OrderBy(l => l.Index1).ThenBy(l => l.Index2).Select(l => Tuple.Create(l.Index1, l.Index2)).ToArray();
        }
    }

    public class Limit
    {
        public int Index1 { get; }
        public int Index2 { get; }
        public double Value { get; }

        public Limit(int index1, int index2, double value)
        {
            Index1 = index1;
            Index2 = index2;
            Value = value;
        }
    }
}
