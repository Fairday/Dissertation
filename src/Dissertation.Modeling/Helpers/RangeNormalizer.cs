using System;

namespace Dissertation.Modeling.Helpers
{
    public class RangeNormalizer
    {
        private readonly double _Range;

        public RangeNormalizer(double range)
        {
            _Range = range;
            Normalize(0);
        }

        public double Normalized { get; private set; }

        public double Normalize(double value, bool includeRight = false)
        {
            if (value == 0)
            {
                return Normalized = 0;
            }
            else if (_Range == 0)
            {
                return Normalized = 0;
            }
            else
            {
                int nRanges = (int)(value / _Range);
                double ret = value - nRanges * _Range;
                if (!includeRight)
                {
                    if (_Range * ret < 0)
                    {
                        ret += _Range;
                    }
                }
                else
                {
                    if (_Range * ret <= 0)
                    {
                        ret += _Range;
                    }
                }
                return Normalized = ret;
            }
        }
        
        static RangeNormalizer()
        {
            DPI = new RangeNormalizer(2 * Math.PI);
        }

        /// <summary>
        /// Нормировщик значения в диапозоне [0, 2PI]
        /// </summary>
        public static RangeNormalizer DPI { get; }
    }
}
