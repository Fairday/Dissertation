using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Model;
using Dissertation.Algorithms.OrbitalMath;
using Dissertation.Algorithms.Resources;
using Dissertation.Modeling.Helpers;
using Dissertation.Modeling.Model.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dissertation.Modeling.Algorithms
{
    /// <summary>
    /// Тип искомого параметра
    /// </summary>
    public enum SearchParameterType
    {
        Inclination, SemimajorAxis
    }

    /// <summary>
    /// Результат определения синхронизированных параметров яруса относительно базового
    /// </summary>
    public struct SyncroNodalTierResult 
    {
        public SyncroNodalTierResult(double deltaSemimajorAxis, Angle deltaInclination, double semimajorAxis, Angle inclination)
        {
            DeltaSemimajorAxis = deltaSemimajorAxis;
            DeltaInclination = deltaInclination;
            SemimajorAxis = semimajorAxis;
            Inclination = inclination;
        }

        public double DeltaSemimajorAxis { get; private set; }
        public Angle DeltaInclination { get; private set; }
        public double SemimajorAxis { get; private set; }
        public Angle Inclination { get; private set; }
    }

    /// <summary>
    /// Класс, основой задачей которого является определение параметров ярусов, путем синхронизации каждого яруса относиотельного базового
    /// </summary>
    public class SynchroNodalParametersCalculator
    {
        double _SemimajorAxis;
        Angle _Inclination;
        int _MaxIterations;
        double C0;

        public SynchroNodalParametersCalculator(double semimajorAxis , Angle inclination, int maxIterations = 100)
        {
            _SemimajorAxis = semimajorAxis;
            _Inclination = inclination;
            _MaxIterations = maxIterations;
            C0 = Calculate_CO(semimajorAxis, inclination);
        }

        /// <summary>
        /// Расчет параметров яруса относительно базового засчет фиксации одного из параметров (a, i) и варьирования второго
        /// </summary>
        /// <param name="deltaSemimajorAxis"></param>
        /// <param name="deltaInclination"></param>
        /// <param name="searchParameterType"></param>
        /// <returns></returns>
        public SyncroNodalTierResult CalculateTierRelativeFirst(double deltaSemimajorAxis, Angle deltaInclination, SearchParameterType searchParameterType)
        {
            if (deltaSemimajorAxis * deltaInclination.Grad != 0) 
            {
                throw new ArgumentException("Заданы оба отклонения, необходимо зафиксировать хотя бы из параметров, чтобы задача поиска второго могла быть решена!");
            }

            if (searchParameterType == SearchParameterType.Inclination) 
            {
                var di = SearchDeltaInclination(deltaSemimajorAxis);
                return new SyncroNodalTierResult(deltaSemimajorAxis, di, _SemimajorAxis + deltaSemimajorAxis, _Inclination + di);
            }
            else 
            {
                var da = SearchDeltaSemimajorAxis(deltaInclination);
                return new SyncroNodalTierResult(da, deltaInclination, _SemimajorAxis + da, _Inclination + deltaInclination);
            }
        }

        /// <summary>
        /// Поиск отклонения параметра большой полуоси
        /// </summary>
        /// <param name="deltaInclination"></param>
        /// <returns></returns>
        double SearchDeltaSemimajorAxis(Angle deltaInclination) 
        {
            double eps = Constants.J2 * Constants.J2 * Constants.J2;
            int iter = 0;
            double b = CalculateSimplified_Da(deltaInclination);
            double a = -b;

            var di = deltaInclination.Rad;
            double fa = Func(a, di);
            double fb = Func(b, di);

            if (fa * fb > 0)
            {   
                //если точка b выбрана неудачно
                if (Math.Abs(fa) < Math.Abs(fb))
                { 
                    //если мы приблизились к решению, то нужно продолжить уменьшать точку a
                    do
                    {
                        a = a - b;
                        fa = Func(a, di);
                        fb = Func(b, di);
                    } 
                    while (fa * fb > 0);
                }
                else
                { 
                    //решение отдалилось, меняем направление
                    a = 2 * b;
                    fa = Func(a, di);
                    fb = Func(b, di);
                    while (fa * fb > 0)
                    {
                        a = a + b;
                        fa = Func(a, di);
                        fb = Func(b, di);
                    }
                }
            }

            if (a > b)
            { 
                //если получлось, что a ушло за b
                Swap(ref a, ref b);
                fa = Func(a, di);
                fb = Func(b, di);
            }
            double x = a;
            double fx = fa;
            while (Math.Abs(b - a) > eps)
            {
                x = 0.5 * (a + b);
                fa = Func(a, di);
                fx = Func(x, di);
                if (fa * fx < 0)
                {
                    b = x;
                }
                else
                {
                    a = x;
                }
                if (++iter > _MaxIterations)
                {
                    break;
                }
            }

            return x;
        }

        /// <summary>
        /// Поиск отклонения параметра наклонения
        /// </summary>
        /// <param name="deltaSemimajorAxis"></param>
        /// <returns></returns>
        Angle SearchDeltaInclination(double deltaSemimajorAxis) 
        {
            double eps = Constants.J2 * Constants.J2 * Constants.J2;
            int iter = 0;
            double b = CalculateSimplified_Di(deltaSemimajorAxis);
            double a = -b;

            var da = deltaSemimajorAxis;
            double fa = Func(da, a);
            double fb = Func(da, b);

            if (fa * fb > 0)
            {
                //если точка b выбрана неудачно
                if (Math.Abs(fa) < Math.Abs(fb))
                { 
                    //если мы приблизились к решению, то нужно продолжить уменьшать точку a
                    do
                    {
                        a = a - b;
                        fa = Func(da, a);
                        fb = Func(da, b);
                    } 
                    while (fa * fb > 0);
                }
                else
                { 
                    //решение отдалилось, меняем направление
                    a = 2 * b;
                    fa = Func(da, a);
                    fb = Func(da, b);
                    while (fa * fb > 0)
                    {
                        a = a + b;
                        fa = Func(da, a);
                        fb = Func(da, b);
                    }
                }
            }

            if (a > b)
            {  
                //если получлось, что a ушло за b
                Swap(ref a, ref b);
                fa = Func(da, a);
                fb = Func(da, b);
            }
            double x = a;
            double fx = fa;
            while (Math.Abs(b - a) > eps)
            {
                iter++;
                x = 0.5 * (a + b);
                fa = Func(da, a);
                fx = Func(da, x);
                if (fa * fx < 0)
                {
                    b = x;
                }
                else
                {
                    a = x;
                }
                if (iter > 100)
                {
                    break;
                }
            }

            return new Angle(x, true);
        }

        public double Precession(double semimajorAxis, Angle inclination) 
        {
            var a = semimajorAxis;
            var i = inclination;
            var denum = a * a * OM.Tdr(a, inclination.Grad);
            var dOmdt1 = -3.0 * Math.PI * Constants.J2 * Constants.Re * Constants.Re * Math.Cos(i.Rad) / denum; // rad/sec
            return dOmdt1 * 3600 * 24 * 180 / Math.PI; //deg/day
        }

        double Calculate_CO(double semimajorAxis, Angle inclination) 
        {
            var a0 = semimajorAxis;
            return Math.Cos(inclination.Rad) / (a0 * a0 * a0 * Math.Sqrt(a0));
        }

        double CalculateSimplified_Da(Angle deltaInclination) 
        {
            var num = Math.Cos((_Inclination + deltaInclination).Rad);
            var den = C0;

            var da = Math.Pow(num / den, 2d / 7d) - _SemimajorAxis;
            return da;
        }

        double CalculateSimplified_Di(double deltaSemimajorAxis) 
        {
            var da = deltaSemimajorAxis;
            var a0 = _SemimajorAxis;
            var arg = C0 * (a0 + da) * (a0 + da) * (a0 + da) * Math.Sqrt(a0 + da);
            var rad = Math.Acos(arg) - _Inclination.Rad;
            return rad;
        }

        double Func(double da, [Rad]double di) 
        {
            var i0 = _Inclination;
            var a0 = _SemimajorAxis;
            double v1 = Math.Cos(i0.Rad + di) / Math.Cos(i0.Rad);
            double v2 =  (OM.Tdr(a0 + da, i0.Grad + di.ToGrad()) * (a0 + da) * (a0 + da)) / (OM.Tdr(a0, i0.Grad) * a0 * a0);
            double result = v1 - v2;
            return (result);
        }

        void Swap(ref double a, ref double b) 
        {
            var temp = b;
            b = a;
            a = temp;
        }
    }
}
