using Dissertation.Algorithms.OrbitalMath;

namespace Dissertation.Modeling.Model.Basics
{
    public static class Guess
    {
        public static double EarchRadius(Angle inclination)
        {
            return OM.AverageEarchRadius(inclination.Grad);
        }
    }
}
