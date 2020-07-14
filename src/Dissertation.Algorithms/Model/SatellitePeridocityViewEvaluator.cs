using Dissertation.Algorithms.Abstractions;
using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.Algorithms.Model;
using Dissertation.Algorithms.Helpers;
using Dissertation.Algorithms.Resources;
using ProcessingModule.Common;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Algorithms.Model
{
    public class SatellitePeridocityViewEvaluator : IMeasurementExecutor<SatelliteOld, SynthesisOrbitalStructureAlgorithms, PeriodicityView>
    {
        public PeriodicityView Execute(IProcessLogger processLogger, SatelliteOld input, SynthesisOrbitalStructureAlgorithms synthesisOrbitalStructureModelAlgorithms)
        {
            processLogger.Log("Начало расчета. Алгоритм: 1 спутник", LogStatus.Information);

            var algorithms = synthesisOrbitalStructureModelAlgorithms;
            var taskParameters = synthesisOrbitalStructureModelAlgorithms.PeriodicityViewTaskParameters;

            var s_pvInputs = new List<double>[taskParameters.ObservationAreaModel.Latitudes.Count];
            var periodicity_pvInputs = new List<double>[taskParameters.ObservationAreaModel.Latitudes.Count];
            var algorithmType = taskParameters.PeriodicityViewAlgorithmType;

            processLogger.Log($"m: {input.OrbitParameters.m}", LogStatus.Information);
            processLogger.Log($"n: {input.OrbitParameters.n}", LogStatus.Information);
            processLogger.Log($"i: {input.OrbitParameters.i}{Symbols.Grad}", LogStatus.Information);

            var idx = 0;
            foreach (var lattitude in taskParameters.ObservationAreaModel.Latitudes)
            {
                processLogger.Log($"-----------------------------------------", LogStatus.Information);
                processLogger.Log($"-----------------------------------------", LogStatus.Information);
                processLogger.Log($"-----------------------------------------", LogStatus.Information);
                processLogger.Log($"Широта: {lattitude.Value}{Symbols.Grad}", LogStatus.Information);

                //Расчет участков инвариантности наблюдений спутника
                processLogger.Log("Расчет участков инвариантности наблюдений спутника", LogStatus.Information);
                var invariantSectors = algorithms.CalculateSatteliteInvariantSectorInfos(processLogger, input, lattitude);

                if (invariantSectors == null)
                    continue;

                processLogger.Log("Расчет потоков наблюдений спутника", LogStatus.Information);
                //Расчет потоков наблюдений спутника
                var observationVariantTimeStreamInfos = invariantSectors.Select(v => new ObservationVariantTimeInfo(v.ObservationVariant, lattitude.Value, null));
                foreach (var timeStreamInfo in observationVariantTimeStreamInfos)
                {
                    processLogger.Log($"Вариант наблюдения: {timeStreamInfo.ObservationVariant.Variant}", LogStatus.Success);
                    foreach (var nodeTime in timeStreamInfo.SortedNodeTimes)
                    {
                        processLogger.Log($"Тип узла: {nodeTime.Node.NodeType}, {nodeTime.Node.Number}", LogStatus.Success);
                        processLogger.Log($"Время прохождения через узел: {nodeTime.T.Round()}с ({(nodeTime.T / 3600).Round()}ч)", LogStatus.Success);
                    }
                    processLogger.Log($"Периодичность обзора: {timeStreamInfo.ViewPeriodicity.Round()}с ({(timeStreamInfo.ViewPeriodicity / 3600).Round()}ч)", LogStatus.Success);
                }

                s_pvInputs[idx] = invariantSectors.Select(v => v.s).ToList();
                periodicity_pvInputs[idx] = observationVariantTimeStreamInfos.Select(s => s.ViewPeriodicity).ToList();
                idx++;
            }

            processLogger.Log("Расчет периодичности обзора", LogStatus.Information);

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
