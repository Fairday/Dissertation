using ATS.WPF.Shell.Model;
using Dissertation.Modules.OrbitalModule.MeasurementModule;

namespace Dissertation.Modules.OrbitalModule
{
    public class OrbitalSystemModule : ModuleBase
    {
        public OrbitalSystemModule(OrbitalSystemMeasurementManager orbitalSystemMeasurementManager)
        {
            OrbitalSystemMeasurementManager = orbitalSystemMeasurementManager;
        }

        public OrbitalSystemMeasurementManager OrbitalSystemMeasurementManager { get => Get(); private set => Set(value); }
    }
}
