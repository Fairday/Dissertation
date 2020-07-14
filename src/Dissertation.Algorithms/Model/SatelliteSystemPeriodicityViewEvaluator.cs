using Dissertation.Algorithms.Abstractions;
using Dissertation.Algorithms.Helpers;
using Dissertation.Algorithms.Model;
using ProcessingModule.Common;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Algorithms.Algorithms.Model
{
    public class SatelliteSystemPeriodicityViewEvaluator : IMeasurementExecutor<SatelliteSystem, SynthesisOrbitalStructureAlgorithms, PeriodicityView>
    {
        public PeriodicityView Execute(IProcessLogger processLogger, SatelliteSystem satteliteSystem, SynthesisOrbitalStructureAlgorithms synthesisOrbitalStructureModelAlgorithms)
        {
            var algorithms = synthesisOrbitalStructureModelAlgorithms;
            var taskParameters = synthesisOrbitalStructureModelAlgorithms.PeriodicityViewTaskParameters;
            var sattelites = satteliteSystem.Sattelites.ToArray();

            var s_pvInputs = new List<double>[taskParameters.ObservationAreaModel.Latitudes.Count];
            var periodicity_pvInputs = new List<double>[taskParameters.ObservationAreaModel.Latitudes.Count];
            var algorithmType = taskParameters.PeriodicityViewAlgorithmType;

            var idx = 0;
            foreach (var lattitude in taskParameters.ObservationAreaModel.Latitudes)
            {
                //Расчет фундаментальных зон
                var fundamentalZones = algorithms.CalculateFundamentalZones(processLogger, sattelites, lattitude.Value);
                //Расчет границ фундаментальных зон
                var fundamentalZonesLimits = new FundamentalZonesLimits(fundamentalZones);
                //Расчет параметров участков инвариантности СС
                var sattelliteSystemInvariantSectorInfos = algorithms.CalculateSatteliteSystemInvariantSectorInfos(fundamentalZonesLimits);
                //Расчет временных сдвигов СС
                var satelliteSystemTimeShifts = algorithms.CalculateSatteliteSystemTimeShifts(fundamentalZones, sattelliteSystemInvariantSectorInfos);
                //Расчет вариантов наблюдения СС (Размеры участков инвариатности наблюдения)
                var satelliteSystemObservationVariants = algorithms.CalculateSatteliteSystemObservationVariants(fundamentalZonesLimits, sattelliteSystemInvariantSectorInfos);
                //Расчет потоков наблюдения СС
                var observationVariantTimeStreamInfos = algorithms.CalculateObservationVariantTimeStreamInfos(lattitude, satelliteSystemObservationVariants, satelliteSystemTimeShifts);

                s_pvInputs[idx] = satelliteSystemObservationVariants.Select(v => v.S).ToList();
                periodicity_pvInputs[idx] = observationVariantTimeStreamInfos.Select(s => s.ViewPeriodicity).ToList();
                idx++;
            }

            if (algorithmType == PeriodicityViewAlgorithmType.Lattitude)
            {
                var algorithm = new LatitudePeriodicityViewAlgorithm(taskParameters.ObservationAreaModel.Latitudes[0]);
                return algorithm.Caclulate(s_pvInputs[0].ToArray(), periodicity_pvInputs[0].ToArray());
            }
            else if (algorithmType == PeriodicityViewAlgorithmType.LattitudeRange)
            {
                var algorithm = new LatitudeRangePeriodicityViewAlgorithm(taskParameters.ObservationAreaModel.Latitudes.ToArray());
                return algorithm.Caclulate(s_pvInputs.To2DArray(), periodicity_pvInputs.To2DArray());
            }
            else
            {
                var algorithm = new EarchAreaPeriodicityViewAlgorithm(taskParameters.ObservationAreaModel.Latitudes.ToArray(), taskParameters.ObservationAreaModel.Longitudes.ToArray());
                return algorithm.Caclulate(s_pvInputs.To2DArray(), periodicity_pvInputs.To2DArray());
            }
        }
    }   
}
