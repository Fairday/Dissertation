namespace Dissertation.Algorithms.Model
{
    public class OrbitParameters
    {
        public int m { get; private set; }
        public int n { get; private set; }
        public double i { get; private set; }

        public OrbitParameters(int m, int n, double i)
        {
            this.m = m;
            this.n = n;
            this.i = i;
        }
    }
}
