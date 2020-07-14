namespace Dissertation.Algorithms.Metrica.MetricaResults
{
    public class LinearWidthMR : MetricaResultBase
    {
        public double i { get; }
        public double m { get; }
        public double n { get; }
        public double l => m / n;

        public LinearWidthMR(double linearWidth, double averageEarthRadius, double angleWidth, int m, int n, double i)
        {
            this.i = i;
            this.m = m;
            this.n = n;
            LinearWidth = linearWidth;
            AverageEarthRadius = averageEarthRadius;
            AngleWidth = angleWidth;
        }

        public double LinearWidth { get; }
        public double AverageEarthRadius { get; }
        public double AngleWidth { get; }

        public override string GetKey()
        {
            return i.ToString() + " " + m.ToString() + " " + n.ToString();
        }
    }
}
