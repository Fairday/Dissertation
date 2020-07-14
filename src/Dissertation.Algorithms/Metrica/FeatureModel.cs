namespace Dissertation.Algorithms.Algorithms.Metrica
{
    public abstract class FeaturePrecedentBase<T>
    { 
        public string Codename { get; }
        public T Value { get; }
        public FeaturePrecedentBase(string codename, T value)
        {
            Codename = codename;
            Value = value;
        }
    }

    public class DoubleFeaturePrecedent : FeaturePrecedentBase<double>
    {
        public DoubleFeaturePrecedent(string codename, double value) : base(codename, value)
        {
        }
    }
}
