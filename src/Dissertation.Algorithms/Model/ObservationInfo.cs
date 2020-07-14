using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.OrbitalMath;

namespace Dissertation.Algorithms.Model
{
    public class ObservationInfo
    {
        public double AngleOfView { get; }
        public double AngleWidth { get; }
        public double AngleOfViewGrad => AngleOfView.ToGrad();
        public double AngleWidthGrad => AngleWidth.ToGrad();
        public ObservationPrinciple ObservationPrinciple { get; }

        public ObservationInfo(double angleOfView, double angleWidth, ObservationPrinciple observationPrinciple)
        {
            AngleOfView = angleOfView;
            AngleWidth = angleWidth;
            ObservationPrinciple = observationPrinciple;
        }
    }
}
