namespace Dissertation.Algorithms.Model
{
    public class OrbitGeometryInfo
    {
        public double AverageHeight { get; }
        public double Radius { get; }

        public OrbitGeometryInfo(double averageHeight, double radius)
        {
            AverageHeight = averageHeight;
            Radius = radius;
        }
    }
}
