namespace Dissertation.Algorithms.Model
{
    public enum PeriodicityViewAlgorithmType
    {
        Lattitude, LattitudeRange, EarthArea
    }

    public class PeriodicityView
    {
    }

    public abstract class PeriodicityViewAlgorithmBase<S, Periodicity>
    {
        public abstract PeriodicityView Caclulate(S s, Periodicity periodicity);
    }

    public class LatitudePeriodicityViewAlgorithm : PeriodicityViewAlgorithmBase<double[], double[]>
    {
        public Latitude Lattitude { get; }

        public LatitudePeriodicityViewAlgorithm(Latitude lattitude)
        {
            Lattitude = lattitude;
        }

        public override PeriodicityView Caclulate(double[] s, double[] t)
        {
            return null;
        }
    }

    public class LatitudeRangePeriodicityViewAlgorithm : PeriodicityViewAlgorithmBase<double[,], double[,]>
    {
        public Latitude[] Lattitudes { get; }

        public LatitudeRangePeriodicityViewAlgorithm(Latitude[] lattitudes)
        {
            Lattitudes = lattitudes;
        }

        public override PeriodicityView Caclulate(double[,] s, double[,] t)
        {
            return null;
        }
    }

    public class EarchAreaPeriodicityViewAlgorithm : PeriodicityViewAlgorithmBase<double[,], double[,]>
    {
        public Latitude[] Lattitudes { get; }
        public Longitude[] Longitudes { get; }

        public EarchAreaPeriodicityViewAlgorithm(Latitude[] lattitudes, Longitude[] longitudes)
        {
            Lattitudes = lattitudes;
            Longitudes = longitudes;
        }

        public override PeriodicityView Caclulate(double[,] s, double[,] t)
        {
            return null;
        }
    }
}
