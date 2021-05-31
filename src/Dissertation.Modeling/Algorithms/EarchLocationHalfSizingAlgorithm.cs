using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using Dissertation.Modeling.Model.SatelliteModel;

namespace Dissertation.Modeling.Algorithms
{
    public class EarchLocationHalfSizingAlgorithm : HalfSizingAlgorithm
    {
        private MoveModelingAlgorithm _MoveModelingAlgorithm;
        private readonly Angle _Inclination;
        private readonly PhasePosition _StartSatellitePosition;
        private readonly EarchPointTrace _EarchPointTrace;

        public EarchLocationHalfSizingAlgorithm(
            MoveModelingAlgorithm moveModelingAlgorithm,
            Angle inclination,
            double error,
            PhasePosition startSatellitePosition,
            EarchPointTrace earchPointTrace,
            int maximumIterations = 1000) : base(error, maximumIterations)
        {
            _MoveModelingAlgorithm = moveModelingAlgorithm;
            _Inclination = inclination;
            _StartSatellitePosition = startSatellitePosition;
            _EarchPointTrace = earchPointTrace;
        }

        public override double Caclulate(double value)
        {
            var phasePosition = _MoveModelingAlgorithm.Move(_StartSatellitePosition, value);
            var currentsatelliteSpeedOnEarchPoint = CoordinateSystemConverter.ProjectSatelliteSpeed(
                _Inclination,
                phasePosition.LongitudeAscentNode,
                phasePosition.LatitudeArgument);
            var output = CoordinateSystemConverter.CosBy(_EarchPointTrace.Location, currentsatelliteSpeedOnEarchPoint);
            if (System.Math.Abs(output) < Dissertation.Modeling.Model.AccuracyModel.AngleAccuracyRad)
                return 0;
            return output;
        }
    }
}
