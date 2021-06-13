using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Model.BallisticTasksComponents;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using Dissertation.Modeling.Model.OrbitalSystem;
using Dissertation.Modeling.Model.SatelliteModel;
using System.Collections.Generic;

namespace Dissertation.Modeling.Model.BallisticTasks
{
    /// <summary>
    /// Интерфейс, представлящий собой результат решения задачи анализа периодичности многоярусной системы
    /// </summary>
    public interface IOrbitalSystemPeriodicityViewAnalyticBallisticTaskResult
    {
        IReadOnlyList<InvariantSector> EarchLocationObservationStreams { get; }
        double MaxPeridocity { get; }
    }

    /// <summary>
    /// Класс, описывающий решение задачи анализа периодичности широты или широтного пояса многоярусной системой
    /// </summary>
    public class OrbitalSystemPeriodicityViewAnalyticBallisticTaskResult : IOrbitalSystemPeriodicityViewAnalyticBallisticTaskResult
    {
        public OrbitalSystemPeriodicityViewAnalyticBallisticTaskResult()
        {
            _EarchLocationObservationStreams = new List<InvariantSector>();
        }

        public void AddSector(InvariantSector sector, ObservationStream observationStream)
        {
            _EarchLocationObservationStreams.Add(sector);
            if (observationStream.Periodicity > MaxPeridocity)
                MaxPeridocity = observationStream.Periodicity;
        }

        List<InvariantSector> _EarchLocationObservationStreams;
        public IReadOnlyList<InvariantSector> EarchLocationObservationStreams => _EarchLocationObservationStreams;
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

        public IOrbitalSystemPeriodicityViewAnalyticBallisticTaskResult SolveAnalytic(Angle latitude)
        {
            var solution = new OrbitalSystemPeriodicityViewAnalyticBallisticTaskResult();

            //1. Определение периода повторяемости трассы
            var systemPeriodicity = _OrbitalSystem.EraTier;
            //2. Определение анализируемой протяженности долготы
            var analysingLongitude = _OrbitalSystem.AnalysingLongitudeDistance;
            //3. Для каждого яруса определяем базовые участки инвариантности
            var tiersBasicInvariantAreas = new Dictionary<Tier, PeriodicityViewResult>();
            foreach (var tier in _OrbitalSystem.Tiers)
            {
                // Создаем задачу расчета базовых участков инвариантности
                var tierInvariantsCalculationTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(tier.Satellites[0]);
                // Результат расчета базовых участков инвариантности для параметров орбиты яруса
                var latitudePeriodicityViewResult = tierInvariantsCalculationTask.CalculateAnalytic(latitude);
                // Сохраняем значение базовых участков инвариантности
                tiersBasicInvariantAreas[tier] = latitudePeriodicityViewResult;
            }

            var borders = new List<double>();
            var unique = new HashSet<double>();

            //4. Проецируем спутника с каждого яруса на межузловое расстояние, определяем границы участков инвариантности с учетом анализируемой долготы системы
            foreach (var ts in tiersBasicInvariantAreas)
            {
                var tier = ts.Key;
                var tierBasicInvariantAreas = ts.Value;
                var tierDxTurn = tier.Orbit.DxNodeGrad;
                var k = (int)(analysingLongitude.Grad / tierDxTurn);

                if (tier.HasDefaultSatellite()) 
                    foreach (var sector in tierBasicInvariantAreas.InvariantSectors)
                        if (unique.Add(sector.Start.Grad))
                            borders.Add(sector.Start.Grad);

                foreach (var satellite in tier.Satellites)
                {
                    // Рассчитываем проекцию спутника на межузловое расстояние собственного яруса
                    SingleSatellitePeriodicityViewAnalyticBallisticTask.CalculateOffset(satellite.Orbit,
                        satellite.PhasePosition, out double timeoffet, out Angle longitude, out double asc);

                    // Необходимо выполнить проекцию полученной долготы на анализируемую долготу спутниковой системы
                    if (longitude != 0)
                    {
                        foreach (var invariantSector in tierBasicInvariantAreas.InvariantSectors)
                        {
                            for (int i = 0; i < k; i++)
                            {
                                var border = invariantSector.Start.Grad + longitude.Grad + tierDxTurn * i;
                                if (border > analysingLongitude.Grad) border -= analysingLongitude.Grad;
                                if (unique.Add(border))
                                    borders.Add(border);
                            }
                        }
                    }
                }
            }

            //5. Добавляем начало анализируемого участка
            if (unique.Add(0d))
                borders.Add(0d);
            //5.1 Добавляем границу анализируемого участка
            if (unique.Add(analysingLongitude.Grad))
                borders.Add(analysingLongitude.Grad);
            //6. Сортируем границы участков инвариантности
            borders.Sort();

            //7. Берем середину участка инвариантности и для каждого спутника рассчитываем аналитический поток 
            // с учетом временного смещения спутника(потоки надо растянуть в соответствии с новым значением периода повторяемости трассы)
            for (int i = 1; i < borders.Count; i++)
            {
                var left = borders[i - 1];
                var right = borders[i];
                //Середина участка инвариантности
                var center = new Angle((right + left) / 2);
                var el = new EarchLocation(latitude, center);

                var analyticStreams = new List<ObservationStream>();
                var analytic_accuracy_Streams = new List<ObservationStream>();

                foreach (var ts in tiersBasicInvariantAreas)
                {
                    var tier = ts.Key;
                    var tierBasicInvariantAreas = ts.Value;
                    var tierInvariantsCalculationTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(tier.Satellites[0]);
                    var moveModelingAlgorithm = new MoveModelingAlgorithm(tier.Orbit);

                    foreach (var satellite in tier.Satellites) 
                    {
                        SingleSatellitePeriodicityViewAnalyticBallisticTask.CalculateOffset(satellite.Orbit,
                            satellite.PhasePosition, el, out double timeoffset, out Angle longitude, out double anq);
                        var k = analysingLongitude.Grad / satellite.Orbit.DxTurnGrad; //?? //longitude.Grad / satellite.Orbit.DxTurnGrad;
                        //var projection = longitude.Grad % satellite.Orbit.DxTurnGrad;
                        var invariantSector = tierBasicInvariantAreas.EntireInSector(longitude/*new Angle(projection)*/);
                        //Аналитический поток
                        var analytic = invariantSector.ObservationStream;
                        //Рассчитываем растянутый аналитический поток в соответствии со значением периода повторяемости трасссы системы
                        //Изменяем период повторяемости трассы у потока
                        analytic.Extend(systemPeriodicity);
                        //Растягиваем поток ?? почему теряются наблюдения?
                        var extendedAnalytic = analytic + k * satellite.Orbit.EraTier;
                        var shiftedAnalytic = extendedAnalytic + timeoffset;
                        analyticStreams.Add(shiftedAnalytic);
                        //Уточненная аналитика
                        var accuracy_analytic = tierInvariantsCalculationTask.CalculateModeling(satellite.Orbit, new EarchLocation(latitude, center), 
                            new PhasePosition(0, 0), moveModelingAlgorithm, satellite.ObservationEquipment.Band);
                        //Рассчитываем растянутый уточненный аналитический поток в соответствии со значением периода повторяемости трасссы системы
                        //Изменяем период повторяемости трассы у потока
                        accuracy_analytic.Extend(systemPeriodicity);
                        //Растягиваем поток
                        var extendedAnalytic_Accuracy = accuracy_analytic + k * satellite.Orbit.EraTier;
                        var shiftedAnalytic_Accuracy = extendedAnalytic + timeoffset;
                        analytic_accuracy_Streams.Add(shiftedAnalytic_Accuracy);
                    }
                }

                //Прямое моделирование
                var modeling = SolveModeling(el);

                var mergedAnalytic = analyticStreams.MergeStreams();
                var mergedAnalytic_Accuracy = analytic_accuracy_Streams.MergeStreams();

                var compareResult_Analytic = ObservationStreamExtensions.CompareStreams(systemPeriodicity, mergedAnalytic, modeling, systemPeriodicity / 8);
                var compareResult_Accuracy_Analytic = ObservationStreamExtensions.CompareStreams(systemPeriodicity, mergedAnalytic_Accuracy, modeling, systemPeriodicity / 8);

                var invs = new InvariantSector(new Angle(left), new Angle(right), mergedAnalytic_Accuracy);

                invs.Analytic_Modeling = compareResult_Analytic;
                invs.Accuracy_Analytic_Modeling = compareResult_Accuracy_Analytic;

                solution.AddSector(invs, mergedAnalytic_Accuracy);
            }    

            return solution;
        }

        /// <summary>
        /// Решение задачи анализа периодичности спутниковой системы с помощью прямого моделирования
        /// </summary>
        public ObservationStream SolveModeling(EarchLocation earchLocation)
        {
            //Потоки наблюдения со всех ярусов спутниковой системы для текущей точки
            var observationStreams = new List<ObservationStream>();

            for (int i = 0; i < _OrbitalSystem.Tiers.Count; i++)
            {
                var tier = _OrbitalSystem.Tiers[i];
                var moveModelingAlgorithm = new MoveModelingAlgorithm(tier.Orbit);
                var anaylysisTierBallisticTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(tier.Satellites[0]);
                foreach (var satellite in tier.Satellites)
                {
                    //Решение задачи анализа периодичности одним спутником для заданной точки на Земле
                    var observationStream = anaylysisTierBallisticTask.CalculateModeling(satellite.Orbit, earchLocation, satellite.PhasePosition, moveModelingAlgorithm, satellite.ObservationEquipment.Band, _OrbitalSystem.EraTier);
                    //Сохраняем поток наблюдения для текущего яруса
                    observationStreams.Add(observationStream);
                }
            }

            var resultStream = observationStreams.MergeStreams();

            return resultStream;
        }
    }
}
