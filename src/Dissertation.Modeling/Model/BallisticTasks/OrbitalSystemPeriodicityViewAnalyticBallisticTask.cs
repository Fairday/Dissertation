using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Model.BallisticTasksComponents;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using System.Collections.Generic;

namespace Dissertation.Modeling.Model.BallisticTasks
{
    /// <summary>
    /// Интерфейс, представлящий собой результат решения задачи анализа периодичности многоярусной системы
    /// </summary>
    public interface IOrbitalSystemPeriodicityViewAnalyticBallisticTaskResult
    {
        IReadOnlyDictionary<EarchLocation, ObservationStream> EarchLocationObservationStreams { get; }
        double MaxPeridocity { get; }
    }

    /// <summary>
    /// Класс, описывающий решение задачи анализа периодичности широты или широтного пояса многоярусной системой
    /// </summary>
    public class OrbitalSystemPeriodicityViewAnalyticBallisticTaskResult : IOrbitalSystemPeriodicityViewAnalyticBallisticTaskResult
    {
        public OrbitalSystemPeriodicityViewAnalyticBallisticTaskResult()
        {
            _EarchLocationObservationStreams = new Dictionary<EarchLocation, ObservationStream>();
        }

        public void AddSubResult(EarchLocation earchLocation, ObservationStream observationStream)
        {
            _EarchLocationObservationStreams[earchLocation] = observationStream;
            if (observationStream.Periodicity > MaxPeridocity)
                MaxPeridocity = observationStream.Periodicity;
        }

        Dictionary<EarchLocation, ObservationStream> _EarchLocationObservationStreams;
        public IReadOnlyDictionary<EarchLocation, ObservationStream> EarchLocationObservationStreams => _EarchLocationObservationStreams;
        public double MaxPeridocity { get; private set; }
    }

    /// <summary>
    /// Класс, описывающий задачу анализа периодичности орбитальной системы
    /// </summary>
    public class OrbitalSystemPeriodicityViewAnalyticBallisticTask
    {
        OrbitalSystem.OrbitalSystem _OrbitalSystem;

        public OrbitalSystemPeriodicityViewAnalyticBallisticTask(OrbitalSystem.OrbitalSystem orbitalSystem)
        {
            _OrbitalSystem = orbitalSystem;
        }

        /// <summary>
        /// Решение задачи анализа периодичности спутниковой системы с помощью прямого моделирования
        /// </summary>
        public IOrbitalSystemPeriodicityViewAnalyticBallisticTaskResult SolveWithModeling(Angle longitudeStep, Angle latitude)
        {
            var taskResult = new OrbitalSystemPeriodicityViewAnalyticBallisticTaskResult();

            var longitudeCurrent = new Angle(0); 
            var longitueStop = _OrbitalSystem.AnalysingLongitudeDistance;

            while (longitudeCurrent.Grad <= longitueStop.Grad)
            {
                //Точка на поверхности Земли
                var earchLocation = new EarchLocation(latitude, longitudeCurrent);
                //Потоки наблюдения со всех ярусов спутниковой системы для текущей точки
                var observationStreams = new ObservationStream[_OrbitalSystem.Tiers.Count];
                for (int i = 0; i < _OrbitalSystem.Tiers.Count; i++)
                {
                    var tier = _OrbitalSystem.Tiers[i];
                    //TODO: Временное решение, считаем, что ярус всегда состоит из 1 спутника,
                    //позже данная задача будет разбиваться на N подзадач, где N - количество спутников
                    var satellite = tier.Satellites[0];
                    var moveModelingAlgorithm = new MoveModelingAlgorithm(satellite.Orbit);
                    var anaylysisTierBallisticTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(satellite);
                    //Решение задачи анализа периодичности одним спутником для заданной точки на Земле
                    var observationStream = anaylysisTierBallisticTask.CalculateModeling(satellite.Orbit, earchLocation, satellite.PhasePosition, moveModelingAlgorithm, satellite.ObservationEquipment.Band, _OrbitalSystem.EraTier);
                    //Сохраняем поток наблюдения для текущегр яруса
                    observationStreams[i] = observationStream;
                }

                var resultStream = observationStreams.SummStreams();
                taskResult.AddSubResult(earchLocation, resultStream);

                longitudeCurrent += longitudeStep;
            }

            return taskResult;
        }
    }
}
