using Dissertation.Algorithms.Algorithms.Model;
using Dissertation.Algorithms.OrbitalMath;

namespace Dissertation.Algorithms.Model
{
    public class NodeTime
    {
        public Node Node { get; }
        public SatelliteOld Satellite { get; }
        public double T { get; private set; }

        public NodeTime(Node node, SatelliteOld sattelite, double lattitude, SatelliteSystemTimeShift[] satelliteSystemTimeShifts)
        {
            Node = node;
            Satellite = sattelite;
            CalculateTime(lattitude, satelliteSystemTimeShifts);
        }

        private void CalculateTime(double lattitude, SatelliteSystemTimeShift[] satelliteSystemTimeShifts)
        {
            T = OM.OverflightNodeTime(Satellite, Node, lattitude, satelliteSystemTimeShifts);
        }
    }
}
