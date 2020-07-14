using ATS.MVVM.Core;
using ATS.MVVM.Helpers;

namespace Dissertation.Model
{
    public class OrbitInfo : VirtualBindableBase
    {
        public int m { get => Get(); set => Set(value); }
        public int n { get => Get(); set => Set(value); }
        public double i { get => Get(); set => Set(value); }
        public double l { get => Get(); set => Set(value); }

        public override void OnInitialized()
        {
            m = 1;
            n = 1;
            l = 1;
            i = 65;

            this.ToFluent().Bind(o => o.m).OnSet((oV, nV) =>
            {
                if (IsCorrectOrbitrRatio(nV, n))
                    return nV;
                else
                    return oV;
            });

            this.ToFluent().Bind(o => o.n).OnSet((oV, nV) =>
            {
                if (IsCorrectOrbitrRatio(m, nV))
                    return nV;
                else
                    return oV;
            });
        }

        private bool IsCorrectOrbitrRatio(int m, int n)
        {
            var ratio = (double)m / (double)n;
            if (ratio >= 1 && ratio <= 33d / 2d)
            {
                l = ratio;
                return true;
            }
            else
                return false;
        }
    }
}
