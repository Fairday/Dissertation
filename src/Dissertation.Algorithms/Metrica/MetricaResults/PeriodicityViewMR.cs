using Dissertation.Algorithms.Model;

namespace Dissertation.Algorithms.Metrica.MetricaResults
{
    public class PeriodicityViewMR : MetricaResultBase
    {
        public double i { get; }
        public double m { get; }
        public double n { get; }
        public double l => m / n;

        public PeriodicityViewMR(ObservationVariant observationVariant, double periodicityView, double lattitude, int m, int n, double i)
        {
            this.i = i;
            this.m = m;
            this.n = n;
            ObservationVariant = observationVariant;
            PeriodicityView = periodicityView;
            Lattitude = lattitude;
        }

        public ObservationVariant ObservationVariant { get; }
        public double PeriodicityView { get; }
        public double Lattitude { get; }

        public override string GetKey()
        {
            return i.ToString() + " " + m.ToString() + " " + n.ToString() + " " + Lattitude.ToString();
        }
    }
}
