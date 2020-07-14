namespace Dissertation.Algorithms.Metrica.MetricaResults
{
    public class AngleOfViewMR : MetricaResultBase
    {
        public double i { get; }
        public double m { get; }
        public double n { get; }
        public double l => m / n;

        public AngleOfViewMR(double angleOfView, double averageEarthRadius, double averageOrbitHeight, int m, int n, double i)
        {
            this.i = i;
            this.m = m;
            this.n = n;
            AngleOfView = angleOfView;
            AverageEarthRadius = averageEarthRadius;
            AverageOrbitHeight = averageOrbitHeight;
        }

        public double AngleOfView { get; }
        public double AverageEarthRadius { get; }
        public double AverageOrbitHeight { get; }

        public override string GetKey()
        {
            return i.ToString() + " " + m.ToString() + " " + n.ToString();
        }
    }
}
