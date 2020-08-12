using ATS.MVVM.Collections;
using ATS.MVVM.Core;
using Dissertation.Modeling.Model.OrbitalModel;
using Dissertation.Modeling.Model.SatelliteModel;
using System.Collections.Generic;
using System.Linq;

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

        public bool HasDefaultSatellite() 
        {
            return Satellites.Any(s => s.PhasePosition.IsZero());
        }
    }
}
