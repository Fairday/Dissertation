using Dissertation.Modeling.Model;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.OrbitalModel;
using Dissertation.Modeling.Model.SatelliteModel;

namespace Dissertation.Modeling.Algorithms
{
    public class MoveModelingAlgorithm
    {
        private readonly Orbit _Orbit;

        public MoveModelingAlgorithm(Orbit orbit)
        {
            _Orbit = orbit;
        }

        /// <summary>
        /// Расчет фазового положения на орбите в момент времени dt с учетом 
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public PhasePosition Move(PhasePosition currentPosition, double dt)
        {
            var newL = currentPosition.LongitudeAscentNode.Rad + _Orbit.DeltaLongitudeAscent * dt;
            var newU = currentPosition.LatitudeArgument.Rad + _Orbit.DeltaLatitudeArgument * dt;
            return new PhasePosition(new Angle(newL, true), new Angle(newU, true));
        }
    }
}