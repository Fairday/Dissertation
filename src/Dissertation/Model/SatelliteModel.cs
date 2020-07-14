using ATS.MVVM.Core;

namespace Dissertation.Model
{
    public class SatelliteModel : VirtualBindableBase
    {
        public double NodeLongitude { get => Get(); set => Set(value); }
        public double LatitudeArgument { get => Get(); set => Set(value); }
        public int Number { get => Get(); set => Set(value); }
    }
}
