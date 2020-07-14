namespace Dissertation.Algorithms.Metrica.MetricaResults
{
    public class AverageOrbitHeightMR : MetricaResultBase
    {
        public double i { get; }
        public double m { get; }
        public double n { get; }
        public double l => m / n;
        public double AverageOrbitHeight { get; }

        public AverageOrbitHeightMR(double i, int m, int n, double averageOrbitHeight)
        {
            this.i = i;
            this.m = m;
            this.n = n;
            AverageOrbitHeight = averageOrbitHeight;
        }

        public override string GetKey()
        {
            return i.ToString() + " " + m.ToString() + " " + n.ToString();
        }
    }
}
