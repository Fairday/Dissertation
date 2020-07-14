using ATS.MVVM.Collections;
using ATS.MVVM.Core;
using Dissertation.Modeling.Model.OrbitalModel;
using Dissertation.Modeling.Model.SatelliteModel;
using System.Collections.Generic;

namespace Dissertation.Modeling.Model.OrbitalSystem
{
    public class Tier : VirtualBindableBase
    {
        public Tier(params Satellite[] satellites)
        {
            Satellites = new ObservableList<Satellite>(satellites);
        }

        public Orbit Orbit => Satellites[0].Orbit;
        public ObservableList<Satellite> Satellites { get => Get(); private set => Set(value); }
    }
}
