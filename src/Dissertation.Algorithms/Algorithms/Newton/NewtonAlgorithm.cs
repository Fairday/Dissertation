using System;

namespace Dissertation.Algorithms.Algorithms.Newton
{
    public class NewtonAlgorithm
    {
        public double Start(double startValue, double accuracy, IFunction function, IFunctionDerivate functionDerivate)
        {
            var prevX = startValue;
            var xi = startValue;
            var eps = accuracy;
            var xn = 0d;
            var fn = 0d;

            do
            {
                prevX = xi;
                xn = xi - function.CalculateFunctionValue(xi) / functionDerivate.CalculateDerivateFunctionValue(xi);
                fn = Math.Abs(function.CalculateFunctionValue(xn));
                xi = xn;
            }
            while (fn > eps && xn > 0);

            if (xn > 0)
                return xn;
            else
                return prevX;
        }
    }
}

    