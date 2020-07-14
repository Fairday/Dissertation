using ATS.MVVM.Core;
using Dissertation.Modeling.Model.Equipment;
using Dissertation.Modeling.Model.OrbitalModel;

namespace Dissertation.Modeling.Model.SatelliteModel
{
    public class Satellite : VirtualBindableBase
    {
        /// <summary>
        /// Класс, описывающий моделирование движения спутника по трассе орбиты
        /// </summary>
        public SatelliteTrace SatelliteTrace { get => Get(); private set => Set(value); }
        /// <summary>
        /// Орбиты спутника
        /// </summary>
        public Orbit Orbit { get => Get(); private set => Set(value); }
        /// <summary>
        /// Модель приборов наблюдения спутника
        /// </summary>
        public IObservationEquipment ObservationEquipment { get => Get(); private set => Set(value); }
        /// <summary>
        /// Начальное фазовое положение спутника
        /// </summary>
        public PhasePosition PhasePosition { get => Get(); private set => Set(value); }

        public Satellite(Orbit orbit, IObservationEquipment observationEquipment, PhasePosition phasePosition)
        {
            PhasePosition = phasePosition;
            SatelliteTrace = new SatelliteTrace(orbit, PhasePosition);
            Orbit = orbit;
            ObservationEquipment = observationEquipment;
        }
    }
}
