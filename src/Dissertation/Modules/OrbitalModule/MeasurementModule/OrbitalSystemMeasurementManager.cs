using ATS.Shell.Core;
using ProcessingModule.Common;

namespace Dissertation.Modules.OrbitalModule.MeasurementModule
{
    public class OrbitalSystemMeasurementManager : MeasuringProcessManagerBase<OrbitalSystemMeasurementContext>
    {
        public OrbitalSystemMeasurementManager(IShell shell, OrbitalSystemMeasurementContext orbitalSystemMeasurementContext, IMeasuringController<OrbitalSystemMeasurementContext> measuringController, 
            IProcessLogger processLogger) : base(shell, measuringController, processLogger)
        {
            MeasurementContext = orbitalSystemMeasurementContext;
        }
    }
}
