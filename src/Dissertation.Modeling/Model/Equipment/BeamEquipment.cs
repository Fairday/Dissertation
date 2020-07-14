using Dissertation.Modeling.Model.Basics;

namespace Dissertation.Modeling.Model.Equipment
{
    public class BeamEquipment : IObservationEquipment
    {
        public BeamEquipment(Angle band)
        {
            Band = band;
        }

        public Angle Band { get; }
    }
}
