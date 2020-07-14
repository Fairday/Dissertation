using ATS.MVVM.Collections;
using ATS.MVVM.Core;
using ATS.MVVM.Helpers;
using Dissertation.Modeling.Model.EarchModel;

namespace Dissertation.Modeling.ViewModel
{
    public class ObservationStreamViewModel : VirtualBindableBase
    {
        public ObservableList<ObservationMomentViewModel> ObservationMoments { get => Get(); private set => Set(value); }
        public int Count => ObservationMoments.Count;
        public double Periodicity { get => Get(); set => Set(value); }
        public EarchLocation EarchLocation { get => Get(); set => Set(value); }

        public ObservationStreamViewModel(EarchLocation earchLocation)
        {
            EarchLocation = earchLocation;

            ObservationMoments = new ObservableList<ObservationMomentViewModel>();
            ObservationMoments.CollectionChanged += (o, e) =>
            {
                this.ToFluent().Raise(t => t.Count);
            };
        }
    }
}
