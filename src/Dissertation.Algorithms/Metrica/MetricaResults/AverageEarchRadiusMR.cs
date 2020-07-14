namespace Dissertation.Algorithms.Metrica.MetricaResults
{
    public class AverageEarchRadiusMR : MetricaResultBase
    {
        public double i { get; }
        public double AverageEarchRadius { get; }

        public AverageEarchRadiusMR(double i, double averageEarchRadius)
        {
            this.i = i;
            AverageEarchRadius = averageEarchRadius;
        }

        public override string GetKey()
        {
            return i.ToString();
        }
    }
}
