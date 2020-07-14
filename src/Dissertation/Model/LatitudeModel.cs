using ATS.MVVM.Core;

namespace Dissertation.Model
{
    public class LatitudeModel : VirtualBindableBase
    {
        public double Value { get => Get(); set => Set(value); }

        public LatitudeModel()
        {
            Value = 0;
        }
    }
}
