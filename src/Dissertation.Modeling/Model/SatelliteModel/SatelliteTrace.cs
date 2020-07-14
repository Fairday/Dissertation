using Dissertation.Algorithms.Resources;
using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using Dissertation.Modeling.Model.OrbitalModel;

namespace Dissertation.Modeling.Model.SatelliteModel
{
    public class SatelliteTrace
    {
        private readonly Orbit _Orbit;
        private readonly MoveModelingAlgorithm _MoveModelingAlgorithm;
        private readonly PhasePosition _StartPosition;

        public SatelliteTrace(Orbit orbit, PhasePosition phasePosition)
        {
            _Orbit = orbit;
            _MoveModelingAlgorithm = new MoveModelingAlgorithm(orbit);
            _StartPosition = phasePosition;
        }

        public void Calculate(double time)
        {
            var averageEarthRadius = Guess.EarchRadius(_Orbit.InputOrbitParameters.InclinationAngle);

            //Проекция подспутниковой точки (используется единичный вектор координат) -> (широта, долгота)
            LocationByCoord = CoordinateSystemConverter.ProjectSatellitePoint(
                _Orbit.InputOrbitParameters.InclinationAngle,
                Position.LongitudeAscentNode,
                Position.LatitudeArgument);

            //Проекция подспутниковой точки (используется единичный вектор скорости) -> (широта, долгота)
            LocationBySpeed = CoordinateSystemConverter.ProjectSatelliteSpeed(
                _Orbit.InputOrbitParameters.InclinationAngle,
                Position.LongitudeAscentNode,
                Position.LatitudeArgument);

            //Географические координаты спутника
            PointXYZ = CoordinateSystemConverter.Calculate(
                averageEarthRadius,
                LocationByCoord.Latitude,
                LocationByCoord.Longitude,
                time, 
                Constants.we);

            //Координаты точки на орбите
            OrbitXYZ = CoordinateSystemConverter.SatelliteLocationToCoordinates(
                _Orbit.InputOrbitParameters.InclinationAngle,
                Position.LongitudeAscentNode,
                Position.LatitudeArgument, _Orbit.Radius);
        }

        public void CalculatePhasePosition(double time)
        {
            Position = _MoveModelingAlgorithm.Move(_StartPosition, time);
        }

        public Vector PointXYZ { get; private set; }
        public Vector OrbitXYZ { get; private set; }
        public PhasePosition Position { get; private set; }
        public EarchLocation LocationByCoord { get; private set; }
        public EarchLocation LocationBySpeed { get; private set; }
    }
}
