using System.Collections.Generic;

namespace Dissertation.Algorithms.Metrica
{
    public abstract class MetricaBase<TFeature, TObject>
    {
        protected TObject @object;
        private int _MetricLevel = 0;

        public MetricaBase(TObject @object)
        {
            this.@object = @object;
        }

        public virtual void EvaluateMetrica(TFeature[][] featuresMatrix)
        {
            OpenPrecedentAreaPreview(featuresMatrix, new List<TFeature>());
        }

        protected abstract void OnNextEvaluate(TFeature[] featureRow);

        protected virtual void OpenPrecedentAreaPreview(TFeature[][] sourceFeaturesMatrix, List<TFeature> creatingFeatureRow)
        {
            var featurePrecedentsSequential = sourceFeaturesMatrix[_MetricLevel];
            foreach (var feature in featurePrecedentsSequential)
            {
                if (sourceFeaturesMatrix.Length != _MetricLevel + 1)
                {
                    creatingFeatureRow.Add(feature);
                    _MetricLevel++;
                    OpenPrecedentAreaPreview(sourceFeaturesMatrix, creatingFeatureRow);
                    creatingFeatureRow.Remove(feature);
                }
                else
                {
                    creatingFeatureRow.Add(feature);
                    OnNextEvaluate(creatingFeatureRow.ToArray());
                    creatingFeatureRow.Remove(feature);
                }
            }
            _MetricLevel--;
        }
    }
}
