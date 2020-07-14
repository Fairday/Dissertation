using Dissertation.Algorithms.Algorithms.Helpers;

namespace Dissertation.Algorithms.Model
{
    public interface ICaptureZone
    {
        double DeltaAlpha { get; }
        double Alpha { get; }
        double AlphaLeft { get; }
        double AlphaRight { get; }
    }

    public class LowerCaptureZone : ICaptureZone
    {
        public double AlphaLeft { get; }
        public double AlphaRight { get; }
        public double Alpha => AlphaLeft + AlphaRight;
        public double AlphaLeftAngle => AlphaLeft.ToGrad();
        public double AlphaRightAngle => AlphaRight.ToGrad();
        public double AlphaAngle => AlphaLeft.ToGrad() + AlphaRight.ToGrad();
        public double DeltaAlpha => AlphaRight - AlphaLeft;

        public LowerCaptureZone(double alphaLeft, double alphaRight)
        {
            AlphaLeft = alphaLeft;
            AlphaRight = alphaRight;
        }
    }

    public class UpperCaptureZone : ICaptureZone
    {
        public double AlphaLeft => Alpha / 2;
        public double AlphaRight => Alpha / 2;
        public double Alpha { get; }
        public double DeltaAlpha => 0;
        public double AlphaLeftAngle => AlphaLeft.ToGrad();
        public double AlphaRightAngle => AlphaRight.ToGrad();
        public double AlphaAngle => Alpha.ToGrad();

        public UpperCaptureZone(double alpha)
        {
            Alpha = alpha;
        }
    }
}
