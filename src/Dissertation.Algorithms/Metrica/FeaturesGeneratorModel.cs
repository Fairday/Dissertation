using Dissertation.Algorithms.Algorithms.Metrica;
using System.Collections.Generic;

namespace Dissertation.Algorithms.Metrica
{
    public abstract class FeaturesPrecedentGeneratorBase<TFeature, P>
        where TFeature : FeaturePrecedentBase<P>
    {
        public P From { get; }
        public P To { get; }
        public P Delta { get; }

        public FeaturesPrecedentGeneratorBase(P from, P to, P delta)
        {
            From = from;
            To = to;
            Delta = delta;
        }

        public abstract TFeature[] Generate();
    }

    public class DoubleRangeFeaturesPrecedentGenerator : FeaturesPrecedentGeneratorBase<DoubleFeaturePrecedent, double>
    {
        public string Codename { get; }

        public DoubleRangeFeaturesPrecedentGenerator(double from, double to, double delta, string codename) : base(from, to, delta)
        {
            Codename = codename;
        }

        public override DoubleFeaturePrecedent[] Generate()
        {
            var features = new List<DoubleFeaturePrecedent>();

            var current = From;
            var stop = To;

            var startFeature = new DoubleFeaturePrecedent(Codename, current);
            features.Add(startFeature);

            current += Delta;

            while (current < stop)
            {
                var feature = new DoubleFeaturePrecedent(Codename, current);
                features.Add(feature);
                current += Delta;
            }

            var stopFeature = new DoubleFeaturePrecedent(Codename, stop);
            features.Add(stopFeature);

            return features.ToArray();
        }
    }
}
