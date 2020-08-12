using Dissertation.Modeling.Model.Basics;

namespace Dissertation.Modeling.Model.SatelliteModel
{
    public struct PhasePosition
    {
        public PhasePosition(Angle longitudeAscentNode, Angle latitudeArgument)
        {
            LongitudeAscentNode = longitudeAscentNode;
            LatitudeArgument = latitudeArgument;
        }

        public PhasePosition(double longitudeAscentNode, double latitudeArgument)
        {
            LongitudeAscentNode = new Angle(longitudeAscentNode);
            LatitudeArgument = new Angle(latitudeArgument);
        }

        public PhasePosition(double longitudeAscentNode, double latitudeArgument, bool asRad)
        {
            LongitudeAscentNode = new Angle(longitudeAscentNode, asRad);
            LatitudeArgument = new Angle(latitudeArgument, asRad);
        }

        public static PhasePosition Default => new PhasePosition(0, 0);
        public Angle LongitudeAscentNode { get; }
        public Angle LatitudeArgument { get; }
        public bool IsZero()
        {
            return LongitudeAscentNode.Grad == 0 && LatitudeArgument.Grad == 0;
        }
    }
}
