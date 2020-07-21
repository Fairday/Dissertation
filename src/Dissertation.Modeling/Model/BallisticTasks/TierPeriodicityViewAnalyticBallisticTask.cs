using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Model.BallisticTasksComponents;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using Dissertation.Modeling.Model.OrbitalSystem;
using Dissertation.Modeling.Model.SatelliteModel;
using System;
using System.Collections.Generic;

namespace Dissertation.Modeling.Model.BallisticTasks
{
    /// <summary>
    /// Класс, описывающий решение задачи анализа периодичности широты ярусом орбитальной системы
    /// </summary>
    public class TierPeriodicityViewAnalyticBallisticTask
    {
        /// <summary>
        /// Анализируемый ярус
        /// </summary>
        Tier _Tier;

        public TierPeriodicityViewAnalyticBallisticTask(Tier tier)
        {
            if (tier == null || tier.Satellites.Count == 0)
                throw new ArgumentNullException(nameof(tier));

            _Tier = tier;
        }

        /// <summary>
        /// Анализ периодичности широты
        /// </summary>
        /// <param name="latitude"></param>
        /// <returns></returns>
        /// 1. Расчет для 0,0
        /// 2. Берем спутник и считаем проекцию его положения на межузловое
        /// 3. Берем участки инвариатности для 0,0 - к началу каждого участка добавляем проекцию со 2-ого шага, складываем в массив данные границы
        /// 4. Выполняем так для каждого спутника
        /// 5. Сортируем границы
        /// 6. Берем середину первого участка из массива
        /// 7. Считаем поток наблюдения для каждого спутника(аналитика, уточненная аналитика, моделирование), считая, что середина участка - это произвольная точка
        /// 8. Объединяем все потоки наблюдения и сравниваем результаты
        /// 9. Выполняем пункты 6-8 для каждого участка инвариантности
        public PeriodicityViewResult CalculateAnalytic(Angle latitude) 
        {
            var singleSatellitePeriodicityViewAnalyticBallisticTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(_Tier.Satellites[0]);
            //Результат расчет базовых участков инвариантности для параметров орбиты яруса
            var latitudePeriodicityViewResult = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateAnalytic(latitude);

            var borders = new List<double>();

            foreach (var invariantSector in latitudePeriodicityViewResult.InvariantSectors)
                borders.Add(invariantSector.Start.Grad);

            foreach (var satellite in _Tier.Satellites)
            {
                singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateOffset(satellite.Orbit,
                    satellite.PhasePosition, out double timeoffet, out Angle longitude, out double asc);

                if (longitude != 0)
                {
                    foreach (var invariantSector in latitudePeriodicityViewResult.InvariantSectors)
                    {
                        var border = invariantSector.Start.Grad + longitude.Grad;
                        if (border > satellite.Orbit.DxNodeGrad) border -= satellite.Orbit.DxNodeGrad;
                        borders.Add(border);
                    }
                }
            }

            //Добавляем меужузловое расстояние
            borders.Add(_Tier.Orbit.DxNodeGrad);
            //Сортируем границы участков инвариантности
            borders.Sort();

            var moveModelingAlgorithm = new MoveModelingAlgorithm(_Tier.Orbit);
            var sectors = new List<InvariantSector>(borders.Count - 1);

            for (int i = 1; i < borders.Count; i++)
            {
                var left = borders[i - 1];
                var right = borders[i];
                //Середина участка инвариантности
                var center = (right + left) / 2;
                var el = new EarchLocation(latitude, new Angle(center));

                var analytic_OS = new List<ObservationStream>(_Tier.Satellites.Count);
                var accuracy_analytic_OS = new List<ObservationStream>(_Tier.Satellites.Count);
                var modeling_OS = new List<ObservationStream>(_Tier.Satellites.Count);

                foreach (var satellite in _Tier.Satellites)
                {
                    singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateOffset(satellite.Orbit,
                        satellite.PhasePosition, el, out double timeoffset, out Angle longitude, out double anq);
                    var invariantSector = latitudePeriodicityViewResult.EntireInSector(longitude);
                    //Аналитика
                    var analytic = invariantSector.ObservationStream + timeoffset;
                    analytic_OS.Add(analytic);
                    //Уточненная аналитика
                    var accuracy_analytic = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(satellite.Orbit,
                                new EarchLocation(latitude, invariantSector.Center),
                                new PhasePosition(0, 0), moveModelingAlgorithm, satellite.ObservationEquipment.Band);
                    var shift_accuracy_analytic = accuracy_analytic + timeoffset;
                    accuracy_analytic_OS.Add(shift_accuracy_analytic);
                    //Прямое моделирование
                    var modeling = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(satellite.Orbit, el,
                                satellite.PhasePosition, moveModelingAlgorithm, satellite.ObservationEquipment.Band);
                    modeling_OS.Add(modeling);
                }

                var merged_analytic = analytic_OS.SummStreams();
                var merged_accuracy_analytic = accuracy_analytic_OS.SummStreams();
                var mrged_modeling = modeling_OS.SummStreams();

                var compareResult_Analytic = ObservationStreamExtensions.CompareStreams(_Tier.Orbit, merged_analytic, mrged_modeling, _Tier.Orbit.EraTurn / 4);
                var compareResult_Accuracy_Analytic = ObservationStreamExtensions.CompareStreams(_Tier.Orbit, merged_accuracy_analytic, mrged_modeling, _Tier.Orbit.EraTurn / 4);

                var invs = new InvariantSector(new Angle(left), new Angle(right), merged_accuracy_analytic);

                invs.Analytic_Modeling = compareResult_Analytic;
                invs.Accuracy_Analytic_Modeling = compareResult_Accuracy_Analytic;

                sectors.Add(invs);
            }

            var result = new PeriodicityViewResult(latitudePeriodicityViewResult.LatitudeType, latitude, sectors.ToArray());
            return result;
        }
    }
}
