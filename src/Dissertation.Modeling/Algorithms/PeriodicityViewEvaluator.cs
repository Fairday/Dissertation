using Dissertation.Modeling.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Modeling.Algorithms
{
    public static class PeriodicityViewEvaluator
    {
        public static double Calculate(IEnumerable<ObservationMomentViewModel> observationMoments)
        {
            double prevTime = 0;
            var maxInterval = 0d;

            foreach (var observationMoment in observationMoments)
            {
                var interval = observationMoment.TSource - prevTime;
                if (interval > maxInterval)
                    maxInterval = (double)interval;
                prevTime = observationMoment.TSource;
            }

            return maxInterval;
        }

        public static double CalculateFast(IEnumerable<ObservationMomentViewModel> observationMoments)
        {
            if (observationMoments.Count() == 0)
            {
                return double.NaN;
            }

            return observationMoments.Max(o => o.DeltaWithPrevious);
        }
    }
}
