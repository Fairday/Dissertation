using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Modeling.Model.OrbitalModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Modeling.Model.BallisticTasksComponents
{
    public enum TwistDirection
    {
        Forward, Back
    }

    public static class ObservationStreamExtensions
    {
        /// <summary>
        /// Расчет количества необходимых витков для смещения на twistCount межузловых расстояний
        /// </summary>
        /// <param name="twistDirection"></param>
        /// <param name="nCoil"></param>
        /// <param name="nDay"></param>
        /// <param name="twistCount"></param>
        /// <returns></returns>
        public static int TwistCount(TwistDirection twistDirection, int nCoil, int nDay, int twistCount)
        {
            if (twistCount == 0)
                return 0;

            var m = nCoil;
            var n = nDay;
            int l = 0;
            double k = 0;
            do
            {
                k = ((twistDirection == TwistDirection.Back ? twistCount : -twistCount) + l * m) / (double)n;
                l++;
            }
            while (!(k > 0 && k.IsInteger()));

            var intK = (int)k;

            if (intK > m)
            {
                var mInclusions = intK % m;
                intK -= m * mInclusions;
            }

            return intK;
        }

        /// <summary>
        /// Поворот потока наблюдения
        /// </summary>
        /// <param name="observationStream"></param>
        /// <param name="eraTurn"></param>
        /// <param name="twistDirection"></param>
        /// <param name="nCoil"></param>
        /// <param name="nDay"></param>
        /// <param name="twistCount"></param>
        /// <returns></returns>
        public static ObservationStream Twist(this ObservationStream observationStream, double eraTurn, TwistDirection twistDirection, int nCoil, int nDay, int twistCount)
        {
            //var m = nCoil;
            //var n = nDay;
            //int l = 0;
            //double k = 0;
            //do
            //{
            //    k = ((twistDirection == TwistDirection.Back ? twistCount : -twistCount) + l * m) / (double)n;
            //    l++;
            //}
            //while (!(k > 0 && k.IsInteger()));

            //var intK = (int)k;

            //if (intK > m)
            //{
            //    var mInclusions = intK % m;
            //    intK -= m * mInclusions;
            //}

            int intK = TwistCount(twistDirection, nCoil, nDay, twistCount);

            ///Поворот потока наблюдения на twistCount
            return observationStream + intK * eraTurn;
        }
        /// <summary>
        /// Сравнение двух потоков
        /// </summary>
        /// <param name="observationStream1"></param>
        /// <param name="observationStream2"></param>
        /// <param name="maximumValidTimeError"></param>
        /// <returns></returns>
        public static ObservationStreamsCompareResult CompareStreams(
            Orbit orbit,
            ObservationStream observationStream1,
            ObservationStream observationStream2,
            double maximumValidTimeError)
        {
            return new ObservationStreamsCompareResult(orbit, observationStream1, observationStream2, maximumValidTimeError);
        }
        /// <summary>
        /// Сравнение двух потоков
        /// </summary>
        /// <param name="eraTier"></param>
        /// <param name="observationStream1"></param>
        /// <param name="observationStream2"></param>
        /// <param name="maximumValidTimeError"></param>
        /// <returns></returns>
        public static ObservationStreamsCompareResult CompareStreams(
            double eraTier,
            ObservationStream observationStream1,
            ObservationStream observationStream2,
            double maximumValidTimeError)
        {
            return new ObservationStreamsCompareResult(eraTier, observationStream1, observationStream2, maximumValidTimeError);
        }
        /// <summary>
        /// Объединение потоков
        /// </summary>
        /// <param name="observationStreams"></param>
        /// <returns></returns>
        public static ObservationStream SummAndSortStreams(this ICollection<ObservationStream> observationStreams)
        {
            if (observationStreams == null || observationStreams.Count == 0)
                throw new ArgumentNullException(nameof(observationStreams));

            if (observationStreams.Count == 1)
                return observationStreams.First();

            ObservationStream result = null;

            foreach (var os in observationStreams)
            {
                if (result == null)
                    result = os;
                else
                    result += os;
            }
            return result;
        }
        /// <summary>
        /// Объединение потоков
        /// </summary>
        /// <param name="observationStreams"></param>
        /// <returns></returns>
        public static ObservationStream MergeStreams(this ICollection<ObservationStream> observationStreams)
        {
            throw new NotImplementedException();
        }
    }

    public class ObservationStreamsCompareResult
    {
        public bool AnalyticMore { get; }
        public bool ModelingMore { get; }
        public bool HaveSimilarObservations { get; }
        public bool IsValid { get;}
        public double MaximumValidTimeError { get; }
        public double MaximumRealTimeError { get; }
        public ObservationMoment MaximumErrorAnalytic { get; }

        public ObservationStreamsCompareResult(
            Orbit orbit, 
            ObservationStream observationStream1,
            ObservationStream observationStream2, 
            double maximumValidTimeError)
        {
            MaximumValidTimeError = maximumValidTimeError;
            if (observationStream1.ObservationMoments.Count == observationStream2.ObservationMoments.Count)
            {
                HaveSimilarObservations = true;
                for (int i = 0; i < observationStream1.ObservationMoments.Count; i++)
                {
                    var obm1 = observationStream1.ObservationMoments[i];
                    var obm2 = observationStream2.ObservationMoments[i];
                    var delta = Math.Min(Math.Abs(obm1 - obm2), Math.Abs(orbit.ModEraTier.Normalize(Math.Abs(obm1.T + obm2.T - orbit.EraTier))));
                    if (delta > MaximumRealTimeError)
                    {
                        MaximumRealTimeError = delta;
                        MaximumErrorAnalytic = obm1;
                    }
                }
                IsValid = HaveSimilarObservations && MaximumRealTimeError <= MaximumValidTimeError;
            }
            else
            {
                if (observationStream1.ObservationMoments.Count > observationStream2.ObservationMoments.Count)
                    AnalyticMore = true;
                else
                    ModelingMore = true;
            }
        }

        public ObservationStreamsCompareResult(
            double eraTier,
            ObservationStream observationStream1,
            ObservationStream observationStream2,
            double maximumValidTimeError)
        {
            MaximumValidTimeError = maximumValidTimeError;
            if (observationStream1.ObservationMoments.Count == observationStream2.ObservationMoments.Count)
            {
                HaveSimilarObservations = true;
                for (int i = 0; i < observationStream1.ObservationMoments.Count; i++)
                {
                    var obm1 = observationStream1.ObservationMoments[i];
                    var obm2 = observationStream2.ObservationMoments[i];
                    var delta = Math.Min(Math.Abs(obm1 - obm2), Math.Abs(Orbit.ModEraTierBuilder(eraTier).Normalize(Math.Abs(obm1.T + obm2.T - eraTier))));
                    if (delta > MaximumRealTimeError)
                    {
                        MaximumRealTimeError = delta;
                        MaximumErrorAnalytic = obm1;
                    }
                }
                IsValid = HaveSimilarObservations && MaximumRealTimeError <= MaximumValidTimeError;
            }
            else
            {
                if (observationStream1.ObservationMoments.Count > observationStream2.ObservationMoments.Count)
                    AnalyticMore = true;
                else
                    ModelingMore = true;
            }
        }
    }

    public class ObservationStream
    {
        public ObservationStream(IEnumerable<ObservationMoment> observationMoments, double eraTier)
        {
            ObservationMoments = observationMoments.ToArray();
            EraTier = eraTier;
            Periodicity = CalculatePeriodicity();
        }

        public ObservationStream(IEnumerable<ObservationMoment> observationMoments, double eraTier, double periodicity)
        {
            ObservationMoments = observationMoments.ToArray();
            EraTier = eraTier;
            Periodicity = periodicity;
        }

        public IReadOnlyList<ObservationMoment> ObservationMoments { get; private set; }
        public double EraTier { get; private set; }
        public int Count => ObservationMoments.Count;
        public double Periodicity { get; }

        public void Extend(double newEraTier) 
        {
            EraTier = newEraTier;
        }

        public static ObservationStream operator +(ObservationStream observationStream, double timeOffset)
        {
            var observationStreamAfterTwist = new ObservationStream(observationStream.ObservationMoments.Select(om => new ObservationMoment(om.T)), observationStream.EraTier);
            foreach (var observationMoment in observationStreamAfterTwist.ObservationMoments)
                observationMoment.Move(timeOffset, observationStreamAfterTwist.EraTier);
            //TODO: implement merge sort
            observationStreamAfterTwist.Sort();
            return observationStreamAfterTwist;
        }

        public static ObservationStream operator +(ObservationStream observationStream1, ObservationStream observationStream2)
        {
            var os1 = observationStream1.ObservationMoments.Select(om => new ObservationMoment(om.T));
            var os2 = observationStream2.ObservationMoments.Select(om => new ObservationMoment(om.T));
            var moments = os1.Union(os2, new ObservationMoment(0)).OrderBy(m => m.T);
            var observationStream = new ObservationStream(moments, observationStream1.EraTier);
            return observationStream;
        }

        public void Sort()
        {
            ObservationMoments = ObservationMoments.OrderBy(om => om.T).ToArray();
        }

        double CalculatePeriodicity()
        {
            double prevTime = 0;
            var maxInterval = 0d;

            if (Count == 1)
            {
                return EraTier;
            }

            foreach (var observationMoment in ObservationMoments)
            {
                var interval = observationMoment.T - prevTime;
                if (interval > maxInterval)
                    maxInterval = (double)interval;
                prevTime = observationMoment.T;
            }

            if (Count > 1)
            {
                var interval = (EraTier - ObservationMoments[ObservationMoments.Count - 1].T) + ObservationMoments[0].T;
                if (interval > maxInterval)
                {
                    maxInterval = interval;
                }
            }

            return maxInterval;
        }
    }
}
