namespace Dissertation.Algorithms.Algorithms.Model
{
    public class SatelliteSystemTimeShift
    {
        public int SatelliteNumber { get; }
        public int InvariantSectorNumber { get; }
        public double deltaTkp { get; private set; }
        public int deltaAjkp { get; private set; }
        public int delataJkp { get; private set; }

        public SatelliteSystemTimeShift(int satelliteNumber, int invariantSectorNumber)
        {
            SatelliteNumber = satelliteNumber;
            InvariantSectorNumber = invariantSectorNumber;
        }

        internal void SetParameters(double tkp, int ajkp, int jkp)
        {
            deltaTkp = tkp;
            deltaAjkp = ajkp;
            delataJkp = jkp;
        }
    }
}
