using System;

namespace Dissertation.Modeling.Model.Basics
{
    public struct Vector
    {
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public static double operator -(Vector p1, Vector p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2));
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        
        public static double CosAngleBetween(Vector p1, Vector p2)
        {
            var value = (p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z) / (p1.Length() * p2.Length());
            return value;
        }
    }
}
