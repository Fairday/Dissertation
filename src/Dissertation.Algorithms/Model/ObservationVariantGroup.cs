using Dissertation.Algorithms.OrbitalMath;
using System.Linq;

namespace Dissertation.Algorithms.Model
{
    public class ObservationVariantGroup
    {
        public ObservationVariant[] ObservationVariants { get; }

        public int InvariantSectorNumber { get; }

        public ObservationVariantGroup(int invariantSectorNumber, ObservationPrinciple observationPrinciple, ObservationVariant[] observationVariants, double s)
        {
            InvariantSectorNumber = invariantSectorNumber;
            ObservationVariants = observationVariants;
            S = s;
            var nodes = ObservationVariants.SelectMany(v => v.NodeSequential).OrderBy(n => n.NodeType).ThenBy(n => n.Number);
            UnionVariant = new ObservationVariant(nodes.ToArray(), OM.ConvertNodesToString(observationPrinciple, nodes));
        }

        public ObservationVariant UnionVariant { get; }

        public double S { get; }
    }
}
