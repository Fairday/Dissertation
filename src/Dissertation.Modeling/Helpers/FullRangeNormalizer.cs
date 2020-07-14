namespace Dissertation.Modeling.Helpers
{
    public class FullRangeNormalizer
    {
        public FullRangeNormalizer(double oldLeft, double oldRight,
            double newLeft, double newRight)
        {
            OldLeft = oldLeft;
            OldRight = oldRight;
            NewLeft = newLeft;
            NewRight = newRight;
        }

        public double OldLeft { get; }
        public double OldRight { get; }
        public double NewLeft { get; }
        public double NewRight { get; }

        public double Normalize(double x)
        {
            var xamp = OldRight - OldLeft;
            var yamp = NewRight - 0.0001 - NewLeft;
            var koef = yamp / xamp;

            var xd = x - OldLeft;
            var yd = xd * koef;
            var y = yd + NewLeft;

            return y;
        }
    }
}
