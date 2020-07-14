using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.Equipment;
using Dissertation.Modeling.Model.OrbitalModel;
using Dissertation.Modeling.Model.OrbitalSystem;
using Dissertation.Modeling.Model.SatelliteModel;

namespace Dissertation.Modeling.Model.Extensions
{
    public static class OrbitalElementExtensions
    {
        public static Orbit CreateOrbit(int nCoil, int nDay, double inclination)
        {
            var orbit = new Orbit(new InputOrbitParameters(nCoil, nDay, inclination));
            return orbit;
        }

        public static Satellite CreateSatellite(this Orbit orbit, double band, double longitudeAscendingNode = 0, double latitudeArgument = 0)
        {
            var equipment = new BeamEquipment(new Angle(band));
            var satellite = new Satellite(orbit, equipment, new PhasePosition(longitudeAscendingNode, latitudeArgument));
            return satellite;
        }

        public static Tier CreateTier(params Satellite[] satellites)
        {
            var tier = new Tier(satellites);
            return tier;
        }

        public static Tier CreateTier(this Satellite satellite)
        {
            var tier = new Tier(satellite);
            return tier;
        }
    }
}
