using ATS.MVVM.Core;
using Dissertation.Modeling.Model.EarchModel;
using System;

namespace Dissertation.Modeling.ViewModel
{
    public class ObservationMomentViewModel : VirtualBindableBase
    {
        public ObservationMomentViewModel(double t, double periodicity)
        {
            TSource = t;
            T = Math.Round(t, 3);
            DeltaWithPrevious = periodicity;
        }

        /// <summary>
        /// Округленное значение (TODO: вынести в UI)
        /// </summary>
        public double T { get => Get(); set => Set(value); }
        public double TSource { get => Get(); set => Set(value); }
        public double DeltaWithPrevious { get => Get(); set => Set(value); }
        public EarchLocation EarchLocation { get => Get(); set => Set(value); }
        public EarchLocation SatelliteEarchLocation { get => Get(); set => Set(value); }
    }
}
