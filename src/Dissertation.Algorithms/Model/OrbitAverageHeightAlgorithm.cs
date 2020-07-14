using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Algorithms.Newton;
using Dissertation.Algorithms.Model;
using Dissertation.Algorithms.OrbitalMath;
using Dissertation.Algorithms.Resources;
using System;

namespace Dissertation.Algorithms.Algorithms.Model
{
    public class OrbitAverageHeightAlgorithm
    {
        private NewtonAlgorithm _NewtonAlgorithm;

        public OrbitAverageHeightAlgorithm(NewtonAlgorithm newtonAlgorithm)
        {
            _NewtonAlgorithm = newtonAlgorithm;
        }

        private class IntermediateFunction : IFunction, IFunctionDerivate
        {
            private readonly OrbitParameters _OrbitParameters;
            private readonly double a;
            private readonly double b;
            private readonly double c;

            public IntermediateFunction(OrbitParameters orbitParameters)
            {
                _OrbitParameters = orbitParameters;

                var m = _OrbitParameters.m;
                var n = _OrbitParameters.n;
                var i = _OrbitParameters.i;

                var l = (double)m / (double)n;
                a = 1.5 * Constants.J2 * l * Math.Cos(i.ToRad());
                b = 0.375 * l * Constants.J2 * Constants.we * Math.Pow(Constants.Re, 1.5) / Math.Sqrt(Constants.mu) * (7 * Math.Cos(i.ToRad()) * Math.Cos(i.ToRad()) - 1);
                c = l * Constants.we * Math.Pow(Constants.Re, 1.5) / Math.Sqrt(Constants.mu);
            }

            public double CalculateFunctionValue(double x)
            {
                return a * Math.Pow(x, 7) - b * Math.Pow(x, 4) - Math.Pow(x, 3) + c;
            }

            public double CalculateDerivateFunctionValue(double x)
            {
                return 7 * a * Math.Pow(x, 6) - 4 * b * Math.Pow(x, 3) - 3 * Math.Pow(x, 2);
            }
        }

        public OrbitGeometryInfo CalculateOrbitAverageHeight(double accuracy, OrbitParameters orbitParameters)
        {
            var function = new IntermediateFunction(orbitParameters);
            var x = _NewtonAlgorithm.Start(StartValue(orbitParameters), accuracy, function, function);
            var r0 = Constants.Re / Math.Pow(x, 2);
            var Hcp = r0 - OM.AverageEarchRadius(orbitParameters.i);
            return new OrbitGeometryInfo(Hcp, r0);
        }

        public OrbitGeometryInfo CalculateOrbitAverageHeight(double accuracy, OrbitParameters orbitParameters, double earchRadius)
        {
            var function = new IntermediateFunction(orbitParameters);
            var x = _NewtonAlgorithm.Start(StartValue(orbitParameters), accuracy, function, function);
            var r0 = Constants.Re / Math.Pow(x, 2);
            var Hcp = r0 - earchRadius;
            return new OrbitGeometryInfo(Hcp, r0);
        }

        private double StartValue(OrbitParameters orbitParameters)
        {
            var r0 = Math.Pow(Constants.mu * Math.Pow(Constants.Tst * orbitParameters.n / (2 * Math.PI * orbitParameters.m), 2), 1d / 3d);
            //var r0 = Math.Pow(Constants.mu * Math.Pow(orbitParameters.n / (Constants.we * orbitParameters.m), 2), 1d / 3d);
            var x0 = Math.Sqrt(Constants.Re / r0);
            return x0;
        }
    }
}
