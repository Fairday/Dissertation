using Dissertation.Algorithms.Algorithms.Helpers;
using System;
using System.Linq;

namespace Dissertation.Modeling.Model.Basics
{
    public static class AngleExtensions
    {
        public static double Sum(params Angle[] angles)
        {
            return angles.Sum(a => a.Rad);
        }
    }

    public struct Angle : IComparable
    {
        public Angle(double angle, bool asRad = false)
        {
            if (asRad)
            {
                Grad = angle.ToGrad();
                Rad = angle;
            }
            else
            {
                Grad = angle;
                Rad = angle.ToRad();
            }
        }

        public static bool operator ==(Angle angle, double value)
        {
            if (angle.Rad.Equals(value))
                return true;
            else
                return false;
        }

        public static bool operator !=(Angle angle, double value)
        {
            if (!angle.Rad.Equals(value))
                return true;
            else
                return false;
        }

        public static bool operator !=(Angle angle1, Angle angle2)
        {
            if (!angle1.Rad.Equals(angle2.Rad))
                return true;
            else
                return false;
        }

        public static bool operator ==(Angle angle1, Angle angle2)
        {
            if (angle1.Rad.Equals(angle2.Rad))
                return true;
            else
                return false;
        }

        public static Angle operator -(Angle angle1, Angle angle2)
        {
            return new Angle(angle1.Grad2PI - angle2.Grad2PI);
        }

        public static Angle operator +(Angle angle1, Angle angle2)
        {
            return new Angle(angle1.Grad2PI + angle2.Grad2PI);
        }

        public static Angle operator /(Angle angle, double value)
        {
            return new Angle(angle.Grad / value);
        }

        public static bool operator <(Angle angle1, Angle angle2)
        {
            return angle1.Grad2PI < angle2.Grad2PI;
        }

        public static bool operator >(Angle angle1, Angle angle2)
        {
            return angle1.Grad2PI > angle2.Grad2PI;
        }

        public static bool operator <=(Angle angle1, Angle angle2)
        {
            return angle1.Grad2PI <= angle2.Grad2PI;
        }

        public static bool operator >=(Angle angle1, Angle angle2)
        {
            return angle1.Grad2PI >= angle2.Grad2PI;
        }

        public override string ToString()
        {
            return Grad.ToString();
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is Angle))
                throw new ArgumentException(nameof(obj));

            var angle = (Angle)obj;
            if (angle.Grad > Grad)
                return -1;
            else if (angle.Grad == angle.Grad)
                return 0;
            else
                return 1;
        }

        public double Grad { get; }
        public double Grad2PI => Dissertation.Modeling.Helpers.RangeNormalizer.DPI.Normalize(Rad).ToGrad();
        public double Rad2PI => Grad2PI.ToRad();
        public double Rad { get; }
    }
}
