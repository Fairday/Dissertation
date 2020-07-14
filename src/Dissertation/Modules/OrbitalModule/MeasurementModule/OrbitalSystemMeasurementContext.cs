using ATS.MVVM.Collections;
using ATS.MVVM.Command;
using ATS.MVVM.Core;
using Dissertation.Model;
using System.Windows.Input;

namespace Dissertation.Modules.OrbitalModule.MeasurementModule
{
    public class OrbitalSystemMeasurementContext : VirtualBindableBase
    {
        public OrbitInfo OrbitalInfo { get => Get(); private set => Set(value); }
        public ObservableList<SatelliteModel> Satellites { get => Get(); private set => Set(value); }
        public ObservableList<LatitudeModel> Latitudes { get => Get(); private set => Set(value); }
        public ICommand AddSatelliteCommand { get => Get(); private set => Set(value); }
        public ICommand RemoveSatelliteCommand { get => Get(); private set => Set(value); }
        public ICommand ForceRemoveSatelliteCommand { get => Get(); private set => Set(value); }
        public ICommand AddLatitudeCommand { get => Get(); private set => Set(value); }
        public ICommand RemoveLatitudeCommand { get => Get(); private set => Set(value); }
        public ICommand ForceRemoveLatitudeCommand { get => Get(); private set => Set(value); }

        public OrbitalSystemMeasurementContext()
        {
            OrbitalInfo = new OrbitInfo();

            Satellites = new ObservableList<SatelliteModel>();
            Latitudes = new ObservableList<LatitudeModel>();

            AddSatelliteCommand = new RelayCommand(() =>
            {
                var sm = new SatelliteModel();
                Satellites.Add(sm);
            });

            RemoveSatelliteCommand = new RelayCommand<SatelliteModel>((sm) =>
            {
                Satellites.Remove(sm);
            }, () => Satellites.SelectedItem != null);

            AddLatitudeCommand = new RelayCommand(() =>
            {
                var lm = new LatitudeModel();
                Latitudes.Add(lm);
            });

            RemoveLatitudeCommand = new RelayCommand<LatitudeModel>((lm) =>
            {
                Latitudes.Remove(lm);
            }, () => Latitudes.SelectedItem != null);


            ForceRemoveLatitudeCommand = new RelayCommand<LatitudeModel>((lm) =>
            {
                Latitudes.Remove(lm);
            });

            ForceRemoveSatelliteCommand = new RelayCommand<SatelliteModel>((sm) =>
            {
                Satellites.Remove(sm);
            });
        }
    }
}
