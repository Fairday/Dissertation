﻿namespace Dissertation.Algorithms.Metrica.MetricaResults
{
    public class InvariantSectorSizeMR : MetricaResultBase
    {
        public double i { get; }
        public double m { get; }
        public double n { get; }
        public double l => m / n;

        public InvariantSectorSizeMR(double s, double alpha, double lattitude, double angleWidth, int m, int n, double i)
        {
            this.i = i;
            this.m = m;
            this.n = n;
            S = s;
            Alpha = alpha;
            Lattitude = lattitude;
            AngleWidth = angleWidth;
        }

        public double S { get; }
        public double Alpha { get; }
        public double Lattitude { get; }
        public double AngleWidth { get; }

        public override string GetKey()
        {
            return i.ToString() + " " + m.ToString() + " " + n.ToString() + " " + Lattitude.ToString();
        }
    }
}
