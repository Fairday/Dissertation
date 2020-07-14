namespace Dissertation.Algorithms.Metrica.MetricaResults
{
    public class AngleWidthMR : MetricaResultBase
    {
        public double i { get; }
        public double Lattitude { get; }
        public double m { get; }
        public double n { get; }
        public double l => m / n;

        public AngleWidthMR(double angleWidth, double averageEarthRadius, double averageOrbitHeight, double angleOfView, int m, int n, double i)
        {
            this.i = i;
            this.m = m;
            this.n = n;
            AngleWidth = angleWidth;
            AverageEarthRadius = averageEarthRadius;
            AverageOrbitHeight = averageOrbitHeight;
            AngleOfView = angleOfView;
        }

        public double AngleWidth { get; }
        public double AverageEarthRadius { get; }
        public double AverageOrbitHeight { get; }
        public double AngleOfView { get; }

        public override string GetKey()
        {
            return i.ToString() + " " + m.ToString() + " " + n.ToString();
        }
    }
}
