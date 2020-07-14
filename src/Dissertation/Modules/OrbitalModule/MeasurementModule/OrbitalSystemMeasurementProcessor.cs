using ProcessingModule.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Dissertation.Basics.Mapping;
using Dissertation.Algorithms.Model;
using System.Collections.Generic;
using Dissertation.Algorithms.Algorithms.Model;
using Dissertation.Model;
using System.Linq;

namespace Dissertation.Modules.OrbitalModule.MeasurementModule
{
    public class OrbitalSystemMeasurementProcessor : IMeasuringController<OrbitalSystemMeasurementContext>
    {
        protected readonly IMapper _Mapper;

        public OrbitalSystemMeasurementProcessor(IMapper mapper)
        {
            _Mapper = mapper;
        }

        public IProcessSettings ProcessSettings => null;

        public Task AfterMeasurement(IProcessLogger processLogger, OrbitalSystemMeasurementContext measurementContext)
        {
            return Task.CompletedTask;
        }

        public Task BeforeMeasurement(IProcessLogger processLogger)
        {
            return Task.CompletedTask;
        }

        public void Prepare(OrbitalSystemMeasurementContext measurementContext) { }

        public Task Reset(IProcessLogger processLogger)
        {
            return Task.CompletedTask;
        }

        public Task Start(IProcessLogger processLogger, OrbitalSystemMeasurementContext measurementContext, CancellationToken cancellationToken, IterationResultWatcher iterationResultWatcher, Dispatcher dispatcher = null)
        {
            return Task.Run(() =>
            {
                ///Параметры орбиты
                var orbitParameters = _Mapper.ToDto<OrbitInfo, OrbitParameters>(measurementContext.OrbitalInfo);
                ///Параметры наблюдаемой области
                var observationAreaModel = new ObservationAreaModel()
                {
                    Latitudes = measurementContext.Latitudes.Select(l => _Mapper.ToDto<LatitudeModel, Latitude>(l)).ToList(),
                };

                ///Совокупность параметров решаемой задачи
                var periodicityViewTaskParameters = new PeriodicityViewTaskParameters(orbitParameters, observationAreaModel);
                ///Набор алгоритмов для решения задачи синтеза орбитальной структуры
                var synthesisOrbitalStructureAlgorithms = new SynthesisOrbitalStructureAlgorithms(periodicityViewTaskParameters);

                var satellites = measurementContext.Satellites.Select(l => _Mapper.ToDto<SatelliteModel, SatelliteOld>(l)).ToList();

                if (observationAreaModel.Latitudes.Count == 0)
                {
                    processLogger.Log("Please set latitudes", LogStatus.Warning);
                    return Task.CompletedTask;
                }

                if (satellites.Count == 0)
                {
                    processLogger.Log("Please set satellites", LogStatus.Warning);
                    return Task.CompletedTask;
                }

                if (satellites.Count == 1)
                {
                    ///Задача №1 - Одиночный спутник
                    ///Инициализация одиночного спутника
                    var satteliteSystem = new SatelliteSystem(orbitParameters)
                    {
                        Sattelites = satellites,
                    };
                    var satelite = satteliteSystem.Sattelites[0];
                    ///Эвалюатор для решения задачи расчета периодического обзора одиночным спутником
                    var SattelitePeridocityViewEvaluator = new SatellitePeridocityViewEvaluator();
                    var sattelitePeriodicityView = SattelitePeridocityViewEvaluator.Execute(processLogger, satelite, synthesisOrbitalStructureAlgorithms);
                    return Task.CompletedTask;
                }
                else
                {
                    ///Задача №2 - СС
                    ///Инициализация спутниковой системы
                    var satteliteSystem = new SatelliteSystem(orbitParameters)
                    {
                        Sattelites = satellites,
                    };
                    ///Эвалюатор для решения задачи расчета периодического обзора СС
                    var satteliteSystemPeriodicityViewEvaluator = new SatelliteSystemPeriodicityViewEvaluator();
                    ///Значение периодического обзора
                    var satteliteSystemPeriodicityView = satteliteSystemPeriodicityViewEvaluator.Execute(processLogger, satteliteSystem, synthesisOrbitalStructureAlgorithms);
                    return Task.CompletedTask;
                }
            });
        }

        public Task Stop(IProcessLogger processLogger, CancellationTokenSource cancellationTokenSource)
        {
            return Task.CompletedTask;
        }
    }
}
