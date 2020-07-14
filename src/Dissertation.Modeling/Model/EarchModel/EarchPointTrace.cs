using Dissertation.Algorithms.Resources;
using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Model.Basics;

namespace Dissertation.Modeling.Model.EarchModel
{
    public class EarchPointTrace
    {
        private readonly Angle _Inclination;

        public EarchPointTrace(Angle inclination, EarchLocation earchLocation)
        {
            _Inclination = inclination;
            Location = earchLocation;
        }

        public void Calculate(double time)
        {
            var averageEarthRadius = Guess.EarchRadius(_Inclination);

            //Позиция точки на земной поверхности с учетом вращения Земли
            PointXYZ = CoordinateSystemConverter.Calculate(
                averageEarthRadius,
                Location.Latitude,
                Location.Longitude,
                time,
                Constants.we);
        }

        public Vector PointXYZ { get; private set; }
        public EarchLocation Location { get; private set; }
    }
}
