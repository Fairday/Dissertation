namespace Dissertation.Algorithms.Model
{
    public class SatelliteOld
    {
        public SatelliteInvariantSectorInfo[] InvariantSectors { get; internal set; }
        public double NodeLongitude { get; private set; }
        public double LatitudeArgument { get; private set; }
        public int Number { get; }
        public OrbitGeometryInfo OrbitGeometryInfo { get; private set; }
        public ObservationInfo ObservationInfo { get; private set; }
        public OrbitParameters OrbitParameters { get; private set; }

        public SatelliteOld()
        {

        }

        public SatelliteOld(double nodeLongitude, double latitudeArgument, int number)
        {
            NodeLongitude = nodeLongitude;
            LatitudeArgument = latitudeArgument;
            Number = number;
        }

        public SatelliteOld(double nodeLongitude, double latitudeArgument, OrbitParameters orbitParameters, int number)
        {
            NodeLongitude = nodeLongitude;
            LatitudeArgument = latitudeArgument;
            OrbitParameters = orbitParameters;
            Number = number;
        }

        internal void SetOrbitalInfo(OrbitParameters orbitParameters)
        {
            OrbitParameters = orbitParameters;
        }

        internal void SetObservationInfo(ObservationInfo observationInfo)
        {
            ObservationInfo = observationInfo;
        }

        internal void SetOrbitGeometryInfo(OrbitGeometryInfo orbitGeometryInfo)
        {
            OrbitGeometryInfo = orbitGeometryInfo;
        }

        internal void SetObservationVariants(SatelliteInvariantSectorInfo[] invariantSectors)
        {
            InvariantSectors = invariantSectors;
        }
    }
}
