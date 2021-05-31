using Dissertation.Algorithms.Resources;
using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Model.Basics;

namespace Dissertation.Modeling.Model.EarchModel
{
    public class EarchPointTrace
    {
        private readonly Angle _Inclination;
        private readonly double _AverageEarthRadius;

        public EarchPointTrace(Angle inclination, EarchLocation earchLocation)
        {
            _Inclination = inclination;
            Location = earchLocation;

            //Расчет среднего радиуса Земли
            _AverageEarthRadius = Guess.EarchRadius(_Inclination);
        }

        public void Calculate(double time)
        {
            //Позиция точки на земной поверхности с учетом вращения Земли
            //Перевод углового положения (доглота и широта) в географическую систему координат на момент времени time
            PointXYZ = CoordinateSystemConverter.Calculate(
                _AverageEarthRadius,
                Location.Latitude,
                Location.Longitude,
                time,
                Constants.we);
        }

        /// <summary>
        /// Проекция точки в географической системе коордиант 
        /// </summary>
        public Vector PointXYZ { get; private set; }
        /// <summary>
        /// Угловое положение точки
        /// </summary>
        public EarchLocation Location { get; private set; }
    }
}