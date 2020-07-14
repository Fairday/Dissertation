using Dissertation.Modeling.Model.Basics;

namespace Dissertation.Modeling.Model.EarchModel
{
    public struct EarchLocation
    {
        public EarchLocation(double latitude, double longitude)
        {
            Latitude = new Angle(latitude);
            Longitude = new Angle(longitude);
        }

        public EarchLocation(Angle latitude, Angle longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Angle Latitude { get; }
        public Angle Longitude { get;}
    }
}
