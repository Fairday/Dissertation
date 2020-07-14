using System.Collections.Generic;

namespace Dissertation.Modeling.Model.BallisticTasksComponents
{
    public class ObservationMoment : IEqualityComparer<ObservationMoment>
    {
        public ObservationMoment(double t)
        {
            T = t;
        }

        public double T { get; private set; }

        public static double operator -(ObservationMoment observationMoment1, ObservationMoment observationMoment2)
        {
            return observationMoment1.T - observationMoment2.T;
        }

        public void Move(double offset, double eraTier)
        {
            var newT = T + offset;
            if (System.Math.Abs(newT - eraTier) <= AccuracyModel.WeakCalculationAccuracy)
            {
                newT = 0;
            }
            else if (newT >= eraTier)
            {
                var mInclusions = (int)System.Math.Truncate(newT / eraTier);
                newT -= eraTier * mInclusions;
            }
            else if (newT < 0)
            {
                newT = newT + eraTier;
            }

            T = newT;
        }

        public override string ToString()
        {
            return T.ToString();
        }

        public bool Equals(ObservationMoment x, ObservationMoment y)
        {
            return System.Math.Abs(x.T - y.T) <= AccuracyModel.WeakCalculationAccuracy;
        }

        public int GetHashCode(ObservationMoment obj)
        {
            return 1;
        }
    }
}
