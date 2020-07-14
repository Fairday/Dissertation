using Dissertation.Modeling.Model.Basics;
using System;

namespace Dissertation.Modeling.Algorithms
{
    public class CircularMotionAlgorithm
    {
        public Vector Calculate(double r, double w, double t)
        {
            var x = r * Math.Cos(w * t);
            var y = r * Math.Sin(w * t);

            return new Vector(x, y, 0);
        }
    }
}
