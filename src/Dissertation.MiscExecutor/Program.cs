using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Resources;
using System;

namespace Dissertation.MiscExecutor
{
    class Program
    {
        static void Main(string[] args)
        {
            var satelliteAngleSpeed = 2 * Math.PI / 5260.173;
            var angleDelta = (16d).ToRad();
            var timeToAngleState = angleDelta / satelliteAngleSpeed;
            var earchScrolledAngle = Constants.we * timeToAngleState;
            var earchScrollingTime = earchScrolledAngle / satelliteAngleSpeed;
            var fullTime = timeToAngleState + earchScrollingTime;
            var x = 341.016 * Constants.we;
            var y = x.ToGrad();
            var z = x / satelliteAngleSpeed + timeToAngleState;
        }
    }
}
