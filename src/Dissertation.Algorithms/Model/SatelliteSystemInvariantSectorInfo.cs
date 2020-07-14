namespace Dissertation.Algorithms.Model
{
    public class SatelliteSystemInvariantSectorInfo
    {
        public double S { get; }
        public double AveragePointLongitude { get; }
        public int Number { get; }
        public FundamentalZonesLimits FundamentalZonesLimits { get; }

        public SatelliteSystemInvariantSectorInfo(double s, double averagePointLongitude, int number, FundamentalZonesLimits fundamentalZonesLimits)
        {
            S = s;
            AveragePointLongitude = averagePointLongitude;
            Number = number;
            FundamentalZonesLimits = fundamentalZonesLimits;
        }
    }
}
