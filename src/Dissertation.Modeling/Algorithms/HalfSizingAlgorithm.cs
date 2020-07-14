using System;

namespace Dissertation.Modeling.Algorithms
{
    public struct ParameterValue
    {
        public ParameterValue(double value, double parameter)
        {
            Value = value;
            Parameter = parameter;
        }

        public double Value { get; set; }
        public double Parameter { get; set; }
    }

    public abstract class HalfSizingAlgorithm
    {
        private readonly double _Accuracy;
        private readonly int _MaximumIterations;
        private int _CurrentIteration;

        public HalfSizingAlgorithm(double error, int maximumIterations = 1000)
        {
            _Accuracy = error;
            _MaximumIterations = maximumIterations;
        }

        public ParameterValue Compute(double a, double b)
        {
            _CurrentIteration = 0;
            return Find(a, b);
        }

        public abstract double Caclulate(double parameter);

        private ParameterValue Find(double left, double right)
        {
            _CurrentIteration++;
            var center = (left + right) / 2;

            var valueLeft = Caclulate(left);
            var valueCenter = Caclulate(center);

            if (_CurrentIteration == _MaximumIterations)
                return new ParameterValue(valueCenter, center);

            if (Math.Abs(valueCenter) < _Accuracy)
            {
                return new ParameterValue(valueCenter, center);
            }
            else
            {
                if (valueLeft * valueCenter < 0)
                {
                    return Find(left, center);
                }
                else
                {
                    return Find(center, right);
                }
            }
        }
    }
}
