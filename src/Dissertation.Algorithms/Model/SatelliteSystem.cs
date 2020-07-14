using System.Collections.Generic;

namespace Dissertation.Algorithms.Model
{
    public class SatelliteSystem
    {
        public OrbitParameters OrbitParameters { get; }

        public SatelliteSystem(OrbitParameters orbitParameters)
        {
            OrbitParameters = orbitParameters;
        }

        private List<SatelliteOld> _Sattelites;
        public List<SatelliteOld> Sattelites
        {
            get => _Sattelites;
            set
            {
                _Sattelites = value;
                if (value != null)
                {
                    foreach (var sattelite in _Sattelites)
                        sattelite.SetOrbitalInfo(OrbitParameters);
                }
            }
        }
    }
}
