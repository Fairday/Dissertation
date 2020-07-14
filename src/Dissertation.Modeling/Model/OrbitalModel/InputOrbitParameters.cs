using ATS.MVVM.Core;
using ATS.MVVM.Helpers;
using Dissertation.Modeling.Model.Basics;

namespace Dissertation.Modeling.Model.OrbitalModel
{
    public class InputOrbitParameters : VirtualBindableBase
    {
        /// <summary>
        /// Число витков
        /// </summary>
        public int NCoil { get => Get(); set => Set(value); }
        /// <summary>
        /// Число суток
        /// </summary>
        public int NDay { get => Get(); set => Set(value); }
        /// <summary>
        /// Наклонение орбиты
        /// </summary>
        public double Inclination { get => Get(); set => Set(value); }

        public Angle InclinationAngle { get => Get(); set => Set(value); }

        public InputOrbitParameters(int nCoil, int nDay, double inclination)
        {
            NCoil = nCoil;
            NDay = nDay;
            Inclination = inclination;
            InclinationAngle = new Angle(inclination);

            this.ToFluent().Bind(o => o.NCoil).OnSet((oV, nV) =>
            {
                if (IsCorrectOrbitrRatio(nV, NDay))
                    return nV;
                else
                    return oV;
            });

            this.ToFluent().Bind(o => o.NDay).OnSet((oV, nV) =>
            {
                if (IsCorrectOrbitrRatio(NCoil, nV))
                    return nV;
                else
                    return oV;
            });

            this.ToFluent().Bind(o => o.Inclination).OnSet((oV, nV) =>
            {
                InclinationAngle = new Angle(nV);
                return nV;
            });
        }

        private bool IsCorrectOrbitrRatio(int m, int n)
        {
            var ratio = (double)m / (double)n;
            if (ratio >= 1 && ratio <= 33d / 2d)
            {
                return true;
            }
            else
                return false;
        }
    }
}
