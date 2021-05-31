using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.OrbitalMath;
using Dissertation.Algorithms.Resources;
using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Model.BallisticTasksComponents;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using Dissertation.Modeling.Model.OrbitalModel;
using Dissertation.Modeling.Model.SatelliteModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation.Modeling.Model.BallisticTasks
{
    public enum LatitudeType
    {
        Equator, Lower, Upper, Pole, None
    }

    /// <summary>
    /// Класс, описывающий решение задачи анализа периодичности широты или широтного пояса одним спутником
    /// </summary>
    public class SingleSatellitePeriodicityViewAnalyticBallisticTaskResult
    {
        public SingleSatellitePeriodicityViewAnalyticBallisticTaskResult()
        {
            LatitudeResults = new List<PeriodicityViewResult>();
        }

        public List<PeriodicityViewResult> LatitudeResults { get; private set; }
    }

    /// <summary>
    /// Класс, описывающий результат анализа периодичности для широты
    /// </summary>
    public class PeriodicityViewResult
    {
        public PeriodicityViewResult(LatitudeType latitudeType, Angle latitude, params InvariantSector[] invariantSectors)
        {
            LatitudeType = latitudeType;
            Latitude = latitude;
            InvariantSectors = invariantSectors;
            ///Есть ненаблюдаемые участки
            if (invariantSectors.FirstOrDefault(i => i.Periodicity == null) != null)
            {
                Periodicity = null;
            }
            else
            {
                Periodicity = invariantSectors.Max(inv => inv.Periodicity);
            }
        }

        public PeriodicityViewResult(LatitudeType latitudeType)
        {
            LatitudeType = latitudeType;
            IsEmpty = true;
        }

        public PeriodicityViewResult()
        {
            IsEmpty = true;
        }

        /// <summary>
        /// Тип широты
        /// </summary>
        public LatitudeType LatitudeType { get; }
        /// <summary>
        /// Исследуемая широта
        /// </summary>
        public Angle Latitude { get; }
        /// <summary>
        /// Участки инвариантности, спроецированные на первую зону [0, DxNode]
        /// </summary>
        public InvariantSector[] InvariantSectors { get; }
        /// <summary>
        /// Максимальная периодичность
        /// </summary>
        public double? Periodicity { get; }
        public bool IsEmpty { get; }

        public InvariantSector EntireInSector(Angle longitude)
        {
            //if (InvariantSectors == null 
            //    || InvariantSectors.Length == 0 
            //    || longitude.Grad < 0
            //    || longitude.Grad >= InvariantSectors[InvariantSectors.Length - 1].Stop.Grad)
            //    return null;

            //Array intervals = Array.CreateInstance(typeof(double), InvariantSectors.Length + 1);
            //for (int i = 0; i < InvariantSectors.Length; i++)
            //    intervals.SetValue(InvariantSectors[i].Start.Grad, i);
            //intervals.SetValue(InvariantSectors[InvariantSectors.Length - 1].Stop.Grad, intervals.Length - 1);
            //var position = Array.BinarySearch(intervals, longitude.Grad);

            //if (position == -1 || position == -intervals.Length - 1)
            //    return null;

            //var sector = InvariantSectors[Math.Abs(position) / 2];


            return SingleSatellitePeriodicityViewAnalyticBallisticTask.FindInVariantSectors(longitude, InvariantSectors);
        }
    }

    /// <summary>
    /// Класс, описывающий участок инвариантности
    /// Для участка инвариантности принимается, что поток наблюдения любой точки этого участка одинаковый, а соответственно и значение периодичности.
    /// Потоки наблюдения внутри участка инвариантности, полученные с помощью аналитических алгоритмов, будут отличаться от потоков наблюдения, полученных
    /// с помощью моделирования. Данная ошибка будет являться методической ошибкой аналитического алгоритма анализа периодичности.
    /// </summary>
    public class InvariantSector
    {
        public InvariantSector(Angle start, Angle stop, ObservationStream observationStream)
        {
            Start = start;
            Stop = stop;
            Length = Stop - Start;
            ObservationStream = observationStream;
        }

        public double? Periodicity => ObservationStream?.Periodicity ?? null;
        public Angle Start { get; set; }
        public Angle Length { get; }
        public Angle Center => (Start + Stop) / 2;
        public Angle Stop { get; set; }
        public ObservationStream ObservationStream { get; }

        public ObservationStreamsCompareResult Analytic_Modeling { get; set; }
        public ObservationStreamsCompareResult Accuracy_Analytic_Modeling { get; set; }

        public override string ToString()
        {
            var am = Analytic_Modeling != null ? Analytic_Modeling.HaveSimilarObservations.ToString() + " " + Analytic_Modeling.IsValid : "null";
            var aam = Accuracy_Analytic_Modeling != null ? Accuracy_Analytic_Modeling.HaveSimilarObservations.ToString() + " " + Accuracy_Analytic_Modeling.IsValid : "null";
            return $"Analytic_Modeling: {am}, Accuracy_Analytic_Modeling: {aam}";
        }
    }

    /// <summary>
    /// Класс, описывающий задачу анализа периодичности широты или широтного пояса одним спутником
    /// </summary>
    public class SingleSatellitePeriodicityViewAnalyticBallisticTask
    {
        private readonly Satellite _Satellite;
        private double _CurrentCalculationLatitudeAccuracy;
        private double _CurrentLatitudeLength;

        public SingleSatellitePeriodicityViewAnalyticBallisticTask(Satellite satellite)
        {
            _Satellite = satellite;
        }

        public SingleSatellitePeriodicityViewAnalyticBallisticTaskResult CalculateAnalyticBelt(Belt belt)
        {
            var analyticResult = new SingleSatellitePeriodicityViewAnalyticBallisticTaskResult();

            foreach (var latitude in belt)
            {
                PeriodicityViewResult periodicityViewResult = CalculateAnalytic(latitude);
                analyticResult.LatitudeResults.Add(periodicityViewResult);
            }

            return analyticResult;
        }

        public PeriodicityViewResult CalculateAnalytic(Angle latitude)
        {
            var satellite = _Satellite;
            var orbit = satellite.Orbit;
            var inclination = orbit.InputOrbitParameters.InclinationAngle;
            var band = satellite.ObservationEquipment.Band;
            PeriodicityViewResult periodicityViewResult = null;
            _CurrentCalculationLatitudeAccuracy = AccuracyModel.AccuracyByLatitude(latitude, orbit.EarchRadius, out _CurrentLatitudeLength);

            if (IsEquatorCase(inclination, band, latitude))
                periodicityViewResult = CalculateEquator(satellite, latitude);
            else if (IsPoleCase(inclination, band, latitude))
                periodicityViewResult = CalculatePole(satellite, latitude);
            else
            {
                var observationPrinciple = OM.DetermineObservationPrinciple(latitude.Grad, inclination.Grad, band.Grad);

                if (observationPrinciple == ObservationPrinciple.Upper)
                    periodicityViewResult = CalculateUpperLatitude(satellite, latitude);
                else if (observationPrinciple == ObservationPrinciple.Lower)
                    periodicityViewResult = CalculateLowerLatitude(satellite, latitude);
            }

            return periodicityViewResult ?? new PeriodicityViewResult(LatitudeType.None);
        }

        public ObservationStream CalculateModeling(
            Orbit orbit,
            EarchLocation earchLocation,
            PhasePosition phasePosition,
            MoveModelingAlgorithm moveModelingAlgorithm,
            Angle band,
            double eraTier = 0)
        {
            if (eraTier == 0)
                eraTier = orbit.EraTier;

            //начальное фазовое положение спутника
            var startSatellitePosition = phasePosition;
            //наклонение орбиты
            Angle inclination = orbit.InputOrbitParameters.InclinationAngle;

            //Создаем объект трассы определения положения точки на поверхности Земли с учетом вращения Земли
            var earchPointTrace = new EarchPointTrace(inclination, earchLocation);
            var satelliteTrace = new SatelliteTrace(orbit, startSatellitePosition);
            var earchLocationHalfSizingAlgorithm = new EarchLocationHalfSizingAlgorithm(
                moveModelingAlgorithm,
                inclination,
                AccuracyModel.CalculationAccuracy,
                startSatellitePosition,
                earchPointTrace);

            int? cosAngleSign = null;
            bool isSignChanged = false;
            double dt = orbit.EraTurn / 8;
            double cosTraverseAngle = 0.0;
            double cosAzimuth = 0.0;
            double currentTime = 0.0;
            double cosEtalonTraverseAngle = Math.Cos(Math.PI / 2 + AccuracyModel.AngleAccuracyRad);
            double cosBand = Math.Cos(band.Rad + AccuracyModel.AngleAccuracyRad);

            var observationMoments = new List<ObservationMoment>();

            do
            {
                satelliteTrace.CalculatePhasePosition(currentTime);
                earchPointTrace.Calculate(currentTime);
                satelliteTrace.Calculate(currentTime);

                cosTraverseAngle = CoordinateSystemConverter.CosBy(earchPointTrace.Location, satelliteTrace.LocationBySpeed);

                var newSign = Math.Sign(cosTraverseAngle);
                if (newSign != cosAngleSign || Math.Abs(cosTraverseAngle) <= 10e-4)
                    isSignChanged = true;
                cosAngleSign = newSign;

                if (Math.Abs(cosTraverseAngle) < Math.Abs(cosEtalonTraverseAngle))
                {
                    cosAzimuth = CoordinateSystemConverter.CosBy(earchPointTrace.Location, satelliteTrace.LocationByCoord);

                    if (cosAzimuth > cosBand)
                    {
                        if (!(Math.Abs(currentTime - eraTier) < AccuracyModel.CalculationAccuracy))
                        {
                            AddObservation(observationMoments, eraTier, currentTime);
                            currentTime += dt;
                        }
                    }
                    else
                    {
                        double previousTime = currentTime - dt;

                        var computedMoment = earchLocationHalfSizingAlgorithm.Compute(previousTime, currentTime);

                        if (computedMoment.Parameter < 0)
                            computedMoment.Parameter = eraTier + computedMoment.Parameter;

                        if ((Math.Abs(currentTime - eraTier) < AccuracyModel.CalculationAccuracy))
                            break;

                        satelliteTrace.CalculatePhasePosition(computedMoment.Parameter);
                        satelliteTrace.Calculate(computedMoment.Parameter);
                        earchPointTrace.Calculate(computedMoment.Parameter);
                        cosAzimuth = CoordinateSystemConverter.CosBy(earchPointTrace.Location, satelliteTrace.LocationByCoord);

                        if (cosAzimuth > cosBand && Math.Abs(computedMoment.Value) < AccuracyModel.CalculationAccuracy)
                            AddObservation(observationMoments, eraTier, computedMoment.Parameter);
                    }
                }
                else if (isSignChanged)
                {
                    double previousTime = currentTime - dt;
                    
                    var computedMoment = earchLocationHalfSizingAlgorithm.Compute(previousTime, currentTime);

                    if (computedMoment.Parameter < 0)
                        computedMoment.Parameter = eraTier + computedMoment.Parameter;

                    if ((Math.Abs(currentTime - eraTier) < AccuracyModel.CalculationAccuracy))
                        break;

                    satelliteTrace.CalculatePhasePosition(computedMoment.Parameter);
                    satelliteTrace.Calculate(computedMoment.Parameter);
                    earchPointTrace.Calculate(computedMoment.Parameter);
                    cosAzimuth = CoordinateSystemConverter.CosBy(earchPointTrace.Location, satelliteTrace.LocationByCoord);

                    if (cosAzimuth > cosBand && Math.Abs(computedMoment.Value) < AccuracyModel.CalculationAccuracy)
                        AddObservation(observationMoments, eraTier, computedMoment.Parameter);
                    isSignChanged = false;
                }

                currentTime += dt;
            }
            while (currentTime < eraTier/* + calculationDeltaTimeTail*/);

            var sorted = observationMoments.OrderBy(m => m.T);
            var observationStream = new ObservationStream(sorted, eraTier);
            return observationStream;
        }

        /// <summary>
        /// Данная функция рассчитывает параметры временного смещения, если спутник имеет ненулевое фазовое состояние. 
        /// Также данный метод возращает значение долготы точки, спроецированное на межузловое расстояние
        /// </summary>
        /// <param name="phasePosition"></param>
        /// <param name="earchLocation"></param>
        /// <param name="timeOffset"></param>
        /// <param name="longitude"></param>
        public static void CalculateOffset(Orbit orbit, PhasePosition phasePosition, EarchLocation earchLocation, out double timeOffset, out Angle longitude, out double ascNodeEquator)
        {
            //Необходимо определить угловое расстояние, которое должен пройти спутник для выхода на экватор
            var latitudeArgumentToEquator = phasePosition.LatitudeArgument.Grad != 0 ? 2 * Math.PI - phasePosition.LatitudeArgument.Rad : 0;
            //Определение времени пролета
            var timeToEquator = latitudeArgumentToEquator / orbit.DeltaLatitudeArgument;
            //Изменение долготы восходящего узла
            var L = phasePosition.LongitudeAscentNode.Rad + orbit.DeltaLongitudeAscent * timeToEquator;
            //Значение долготы восходящего узла в момент выхода на экватор
            var longitudeAscentNode = ascNodeEquator = L;
            //Долготная раздница между точкой и спутником
            var azimuth = earchLocation.Longitude.Rad - longitudeAscentNode;
            if (azimuth < 0)
            {
                azimuth += Math.PI * 2;
                //azimuth = Math.Abs(azimuth);
            }
            //Необходимое количество межузловых поворотов для попадания точки в межузловое расстояние (проецирование), ждем пока Земля прокрутится
            var twists = (int)Math.Truncate(azimuth / orbit.DxNode);
            //Необходимое количество межвитковых расстояний для попадания точки в межузловое расстояние (проецирование)
            var intK = ObservationStreamExtensions.TwistCount(TwistDirection.Forward,
                orbit.InputOrbitParameters.NCoil, orbit.InputOrbitParameters.NDay, twists);
            //Проекция долготы точки на межузлове расстояние
            var location = azimuth % orbit.DxNode;
            //Финальное временное смещение
            timeOffset = timeToEquator + intK * orbit.EraTurn;
            longitude = new Angle(location, true);
        }

        /// <summary>
        /// Данная функция рассчитывает параметры временного смещения, если спутник имеет ненулевое фазовое состояние. 
        /// Также данный метод возращает значение долготы спутника, спроецированное на межузловое расстояние
        /// </summary>
        /// <param name="phasePosition"></param>
        /// <param name="earchLocation"></param>
        /// <param name="timeOffset"></param>
        /// <param name="longitude"></param>
        public static void CalculateOffset(Orbit orbit, PhasePosition phasePosition, out double timeOffset, out Angle longitude, out double ascNodeEquator)
        {
            //Необходимо определить угловое расстояние, которое должен пройти спутник для выхода на экватор
            var latitudeArgumentToEquator = phasePosition.LatitudeArgument.Grad != 0 ? 2 * Math.PI - phasePosition.LatitudeArgument.Rad : 0;
            //Определение времени пролета
            var timeToEquator = latitudeArgumentToEquator / orbit.DeltaLatitudeArgument;
            //Изменение долготы восходящего узла
            var L = phasePosition.LongitudeAscentNode.Rad + orbit.DeltaLongitudeAscent * timeToEquator;
            //Значение долготы восходящего узла в момент выхода на экватор
            var longitudeAscentNode = ascNodeEquator = L;
            if (longitudeAscentNode < 0)
            {
                longitudeAscentNode += Math.PI * 2;
                //azimuth = Math.Abs(azimuth);
            }
            //Проекция положения спутника на межузлове расстояние
            var location = longitudeAscentNode % orbit.DxNode;
            //Финальное временное смещение
            timeOffset = timeToEquator;
            longitude = new Angle(location, true);
        }

        void AddObservation(IList<ObservationMoment> moments, double eraTier, double T)
        {
            if (System.Math.Abs(T - eraTier) <= AccuracyModel.WeakCalculationAccuracy)
            {
                T = 0;
            }
            else if (T >= eraTier)
            {
                var mInclusions = (int)System.Math.Truncate(T / eraTier);
                T -= eraTier * mInclusions;
            }
            else if (T < 0)
            {
                T = T + eraTier;
            }

            var moment = new ObservationMoment(T);
            if (!moments.Contains(moment, moment))
            {
                moments.Add(moment);
            }
        }

        /// <summary>
        /// Расчет периодичности для нижних широт
        /// </summary>
        /// <param name="satellite"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        PeriodicityViewResult CalculateLowerLatitude(Satellite satellite, Angle latitude)
        {
            var orbit = satellite.Orbit;
            var m = orbit.InputOrbitParameters.NCoil;
            var n = orbit.InputOrbitParameters.NDay;
            var inclination = orbit.InputOrbitParameters.InclinationAngle;
            ///Захваты для нижних широт
            var captureZone = OM.CalculateCaptureZone(ObservationPrinciple.Lower, latitude.Grad, inclination.Grad, satellite.ObservationEquipment.Band.Rad, m, n);
            ///Начало захвата на восх. узле
            double La = 0d;
            ///Начало захвата на нисх. узле
            double LaDesc = 0d;
            ///Угол до пересечения (восходящий узел)
            var angleDistanceToLatitude = Math.Asin(Math.Sin(latitude.Rad) / Math.Sin(inclination.Rad));
            ///Время до пересечения (восходящий узед)
            var firstAscendingNodeLatitudeTime = angleDistanceToLatitude / orbit.DeltaLatitudeArgument;
            ///Время до пересечения (нисходящий узел)
            var firstDescendingNodeLatitudeTime = orbit.EraTurn / 2 - firstAscendingNodeLatitudeTime;
            if (inclination.Grad <= 90)
            {
                ///Угол разворота с учетом вращения Земли
                var sweepAngle = OM.SweepAngle(n, m, latitude.Grad, inclination.Grad).ToGrad();
                ///Поворот Земли за время полета спутника до первого восходящего узла
                var earchOffsetFirstAscendingNode = Constants.we * firstAscendingNodeLatitudeTime.ToGrad();
                ///Прецессия орбиты за время полета спутника до первого восходящего узла (Рад)
                var orbitPrecessionFirstAscendingNode = orbit.OrbitPrecession * firstAscendingNodeLatitudeTime / orbit.EraTurn;
                ///Прецессия орбиты за время полета спутника до первого восходящего узла (Град)
                var orbitPrecessionFirstAscendingNodeGrad = orbitPrecessionFirstAscendingNode.ToGrad();
                ///Середина участка зоны захватов на восходящем узле
                var Lb = Math.Asin(1 / Math.Tan(inclination.Rad) * Math.Tan(latitude.Rad)).ToGrad() - earchOffsetFirstAscendingNode + orbitPrecessionFirstAscendingNodeGrad;
                ///Начало участка
                La = Lb - captureZone.AlphaLeft.ToGrad();
                ///Середина участка зоны захватов на нисходящем узле
                var LbDesc = Lb + sweepAngle;
                ///Начало участка
                LaDesc = LbDesc - captureZone.AlphaRight.ToGrad();
            }
            else
            {
                ///Захваты, рассчитанные по форулам верхих широт
                var captureZoneUpperByLower = OM.CalculateCaptureZone(ObservationPrinciple.Upper, latitude.Grad, inclination.Grad, satellite.ObservationEquipment.Band.Rad, m, n);
                ///Долгота середины зоны захвата широты
                var captureZoneCenter = 3 * Math.PI / 2 - orbit.EraTurn / 4 * Constants.we;
                var ascStart = captureZoneCenter - captureZoneUpperByLower.AlphaRight;
                La = ascStart.ToGrad();
                var descStart = captureZoneCenter + captureZoneUpperByLower.AlphaRight - captureZone.Alpha;
                LaDesc = descStart.ToGrad();
                var temp = firstAscendingNodeLatitudeTime;
                firstAscendingNodeLatitudeTime = firstDescendingNodeLatitudeTime;
                firstDescendingNodeLatitudeTime = temp;
            }
            ///Максимальное число узлов и в "Н", и в "В"
            var qmax = OM.Qmax(orbit.DxNode, captureZone);
            ///Расчет остатка деления полного захвата на межузловое расстояние
            var u = OM.U(captureZone, qmax, orbit.DxNode);
            #region Ю.Н расчет участков инвариантности
            //var z = OM.Z(captureZone, n, m, latitude.Grad, inclination.Grad);
            //var ksi = OM.Ksi(captureZone, n, m, latitude.Grad, inclination.Grad);
            //var variant = OM.FindObservationVariantIndex(m, u, ksi);
            //var p = OM.LowerNodeNumberIn(n, m, latitude.Grad, inclination.Grad);
            //var isp = new InvariantSectorsParameters()
            //{
            //    variantIndex = variant,
            //    p = p,
            //    qmax = qmax,
            //    u = u,
            //    z = z,
            //    ksi = ksi,
            //};
            /////Определение участков инвариантности потоков наблюдения (расчет по ЮН, уйти от этого расчета)
            //var invariantSectorsOld = OM.GetInvariantSectors(ObservationPrinciple.Lower, latitude.Grad, Inclination0(inclination).Grad, satellite.ObservationEquipment.Band.Grad, m, isp);
            #endregion
            ///Расчет участков инвариантности для восходящего узла
            var invariantSectors_Asc = CalculateInvariantSectors(orbit, qmax, u, new Angle(La), firstAscendingNodeLatitudeTime);
            ///Расчет участков инвариантности для нисходящего узла
            var invariantSectors_Desc = CalculateInvariantSectors(orbit, qmax, u, new Angle(LaDesc), firstDescendingNodeLatitudeTime);
            ///Выполним объединение множеств invariantSectors_Asc и invariantSectors_Desc
            var combinedInvariantSectors = UnionInvariantSectors(invariantSectors_Asc, invariantSectors_Desc);
            ///Финальные участки инвариантности
            var invariantSectors = new List<InvariantSector>();
            foreach (var invariantSector in combinedInvariantSectors)
            {
                var observationStreamFromAsc = FindObservationStreamInVariantSectors(invariantSector.Center, invariantSectors_Asc);
                var observationStreamFromDesc = FindObservationStreamInVariantSectors(invariantSector.Center, invariantSectors_Desc);
                var combinedObservationStream = observationStreamFromAsc + observationStreamFromDesc;
                var finalInvariantSector = new InvariantSector(invariantSector.Start, invariantSector.Stop, combinedObservationStream);
                invariantSectors.Add(finalInvariantSector);
            }
            ///Результат решения задачи анализа периодичности
            ///Проверка на размеры участков инвариантонсти, не несущие никакой аналиической пользы
            var periodicityViewResult = new PeriodicityViewResult(LatitudeType.Lower, latitude, CheckInvariantSectorLengths(invariantSectors));
            return periodicityViewResult;
        }

        /// <summary>
        /// Объединение участков инвариантности
        /// </summary>
        /// <param name="invariantSectors1"></param>
        /// <param name="invariantSectors2"></param>
        /// <returns></returns>
        InvariantSector[] UnionInvariantSectors(InvariantSector[] invariantSectors1, InvariantSector[] invariantSectors2)
        {
            var combined = invariantSectors1.Concat(invariantSectors2).OrderBy(o => o.Stop.Grad).ToArray();
            var result = new List<InvariantSector>();

            for (int i = 0; i < combined.Length; i++)
            {
                InvariantSector invariantSector = null;
                if (i == 0)
                {
                    invariantSector = new InvariantSector(new Angle(0), combined[i].Stop, null);
                }
                else
                {
                    invariantSector = new InvariantSector(combined[i - 1].Stop, combined[i].Stop, null);
                }
                if (invariantSector.Length != 0)
                    result.Add(invariantSector);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Определение в какой участок инвариантности входит точка
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="invariantSectors"></param>
        /// <returns></returns>
        ObservationStream FindObservationStreamInVariantSectors(Angle longitude, InvariantSector[] invariantSectors)
        {
            for (int i = 0; i < invariantSectors.Length; i++)
            {
                if (invariantSectors[i].Start <= longitude && invariantSectors[i].Stop > longitude)
                {
                    return invariantSectors[i].ObservationStream;
                }
            }

            throw new Exception("InvariantSector not found");
        }
        /// <summary>
        /// Определение в какой участок инвариантности входит точка
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="invariantSectors"></param>
        /// <returns></returns>
        public static InvariantSector FindInVariantSectors(Angle longitude, InvariantSector[] invariantSectors)
        {
            for (int i = 0; i < invariantSectors.Length; i++)
            {
                if (invariantSectors[i].Start <= longitude && invariantSectors[i].Stop > longitude)
                {
                    return invariantSectors[i];
                }
            }

            throw new Exception("InvariantSector not found");
        }
        /// <summary>
        /// Расчет периодичности для верхих широт
        /// </summary>
        /// <param name="satellite"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        PeriodicityViewResult CalculateUpperLatitude(Satellite satellite, Angle latitude)
        {
            var orbit = satellite.Orbit;
            var m = orbit.InputOrbitParameters.NCoil;
            var n = orbit.InputOrbitParameters.NDay;
            var inclination = orbit.InputOrbitParameters.InclinationAngle;
            ///Время полета спутника до вертекса орбиты
            var timeToVertex = orbit.EraTurn / 4;
            ///Долгота середины зоны захвата широты
            var captureZoneCenter = (inclination.Grad <= 90 ? Math.PI / 2 : 3 * Math.PI / 2) - orbit.EraTurn / 4 * Constants.we;
            ///Захваты для верхних широт
            var captureZone = OM.CalculateCaptureZone(ObservationPrinciple.Upper, latitude.Grad, inclination.Grad, satellite.ObservationEquipment.Band.Rad, m, n);
            ///Долгота начала зоны захвата
            var captureZoneStart = new Angle(captureZoneCenter - captureZone.AlphaLeft, true);
            ///Максимальное число узлов и в "Н", и в "В"
            var qmax = OM.Qmax(orbit.DxNode, captureZone);
            ///Расчет остатка деления полного захвата на межузловое расстояние
            var u = OM.U(captureZone, qmax, orbit.DxNode);
            #region Ю.Н расчет участков инвариантности
            //var isp = new InvariantSectorsParameters()
            //{
            //    qmax = qmax,
            //    u = u,
            //};
            /////Определение участков инвариантности потоков наблюдения (расчет по ЮН, уйти от этого расчета)
            //var invariantSectorsOld = OM.GetInvariantSectors(ObservationPrinciple.Upper, latitude.Grad, inclination.Grad, satellite.ObservationEquipment.Band.Grad, m, isp);
            #endregion
            ///Расчет участков инвариантности
            var invariantSectors = CalculateInvariantSectors(orbit, qmax, u, captureZoneStart, timeToVertex);
            ///Результат решения задачи анализа периодичности
            var periodicityViewResult = new PeriodicityViewResult(LatitudeType.Upper, latitude, invariantSectors.ToArray());
            return periodicityViewResult;
        }

        /// <summary>
        /// Расчет участков инвариантности захвата широты
        /// Данный алгоритм позволяет определить участки инвариантности с помощью алгоритма для одной зоны захвата на широте
        /// Данный алгоримт используется для определения периодичности для верхних широт, а также и для нижних (отдельно для захвата на восходящем и на нисходящем)
        /// </summary>
        /// <param name="orbit"></param>
        /// <param name="qmax"></param>
        /// <param name="u"></param>
        /// <param name="captureZoneStart"></param>
        /// <param name="timeShift"></param>
        /// <returns></returns>
        InvariantSector[] CalculateInvariantSectors(Orbit orbit, int qmax, double u, Angle captureZoneStart, double timeShift)
        {
            var m = orbit.InputOrbitParameters.NCoil;
            var n = orbit.InputOrbitParameters.NDay;
            var s1 = u.ToGrad();
            var s2 = orbit.DxNodeGrad - s1;
            ///Поток наблюдения первого участка инвариантности (Qmax)
            var observationMoments1 = CreateObservationMoments(qmax, orbit, timeShift);
            var observationStream1 = new ObservationStream(observationMoments1, orbit.EraTier);
            ///Поток наблюдения второго участа инвариантности (Qmax - 1)
            var observationMoments2 = CreateObservationMoments(qmax - 1, orbit, timeShift);
            var observationStream2 = new ObservationStream(observationMoments2, orbit.EraTier);
            ///Проецирование полученных участков инвариантности на [0, DxNode)
            ///Кол-во необходимых поворотов для первого участка инвариантности
            var qS1 = E(captureZoneStart.Rad2PI, orbit.DxNode) + 1;
            ///Кол-во необходимых поворотов для второго участка инвариантности
            var qS2 = qS1;
            ///Поворот первого участка инвариантности
            var observationStream1AfterTwist = observationStream1.Twist(orbit.EraTurn, TwistDirection.Back, m, n, qS1);
            var qS1AfterTwist = captureZoneStart.Grad2PI - qS1 * orbit.DxNodeGrad;
            ///Поворот второго участка инвариантности
            var observationStream2AfterTwist = observationStream2.Twist(orbit.EraTurn, TwistDirection.Back, m, n, qS2);
            var qS2AfterTwist = captureZoneStart.Grad2PI + u.ToGrad() - qS2 * orbit.DxNodeGrad;
            ///Описание участков инвариантности после проецирования
            var invariantSectors = new List<InvariantSector>();
            if (qS2AfterTwist <= 0)
            {
                var _s1 = new Angle(qS2AfterTwist + s2);
                if (_s1 != 0)
                {
                    var _s1_observationStream = observationStream2AfterTwist;
                    if (Math.Abs(_s1.Grad) <= AccuracyModel.CalculationAccuracy)
                    {
                        _s1 = new Angle(0);
                    }
                    var invariantSector = new InvariantSector(new Angle(0), _s1, _s1_observationStream);
                    invariantSectors.Add(invariantSector);
                }

                var _s2 = new Angle(s1);
                if (_s2 != 0)
                {
                    var _s2_observationStream = observationStream1AfterTwist.Twist(orbit.EraTurn, TwistDirection.Forward, m, n, 1);
                    var invariantSector = new InvariantSector(_s1, _s1 + _s2, _s2_observationStream);
                    invariantSectors.Add(invariantSector);
                }

                var _s3 = orbit.DxNodeGrad - _s2.Grad - _s1.Grad;
                if (_s3 != 0)
                {
                    var _s3_observationStream = observationStream2AfterTwist.Twist(orbit.EraTurn, TwistDirection.Forward, m, n, 1);
                    var invariantSector = new InvariantSector(_s1 + _s2, new Angle(orbit.DxNodeGrad), _s3_observationStream);
                    invariantSectors.Add(invariantSector);
                }
            }
            else
            {
                var _s1 = new Angle(qS1AfterTwist + s1);
                if (_s1 != 0)
                {
                    var _s1_observationStream = observationStream1AfterTwist;
                    var invariantSector = new InvariantSector(new Angle(0), _s1, _s1_observationStream);
                    invariantSectors.Add(invariantSector);
                }

                var _s2 = new Angle(s2);
                if (_s2 != 0)
                {
                    var invariantSector = new InvariantSector(_s1, _s1 + _s2, observationStream2AfterTwist);
                    invariantSectors.Add(invariantSector);
                }

                var _s3 = orbit.DxNodeGrad - _s2.Grad - _s1.Grad;
                if (_s3 != 0)
                {
                    var _s3_observationStream = observationStream1AfterTwist.Twist(orbit.EraTurn, TwistDirection.Forward, m, n, 1);
                    var invariantSector = new InvariantSector(_s1 + _s2, new Angle(orbit.DxNodeGrad), _s3_observationStream);
                    invariantSectors.Add(invariantSector);
                }
            }

            if (Math.Abs(invariantSectors.Sum(i => i.Length.Grad) - orbit.DxNodeGrad) > AccuracyModel.AngleAccuracyGrad)
            {
                throw new Exception("Сумма длин участков инвариантности не может быть, чем межузловое расстояние");
            }
            ///Проверка на размеры участков инвариантонсти, которые не представляют никакой аналиической пользы
            return CheckInvariantSectorLengths(invariantSectors);
        }

        /// <summary>
        /// Проверка размеров участков инвариантности
        /// </summary>
        /// <param name="invariantSectors"></param>
        /// <returns></returns>
        InvariantSector[] CheckInvariantSectorLengths(IList<InvariantSector> invariantSectors)
        {
            var smallSizeSectors = new List<InvariantSector>();
            var usefulSectors = new List<InvariantSector>();
            var count = invariantSectors.Count;

            foreach (var invariantSector in invariantSectors)
            {
                var sectorLength = invariantSector.Length.Rad * _CurrentLatitudeLength * 1000;
                if (sectorLength < _CurrentCalculationLatitudeAccuracy)
                    smallSizeSectors.Add(invariantSector);
                else
                    usefulSectors.Add(invariantSector);
            }

            foreach (var invariantSector in smallSizeSectors)
            {
                if (count > 1)
                {
                    var idx = invariantSectors.IndexOf(invariantSector);
                    if (idx == 0)
                    {
                        invariantSectors[idx + 1].Start = invariantSector.Start;
                    }
                    else if (idx == count - 1)
                    {
                        invariantSectors[idx - 1].Stop = invariantSector.Stop;
                    }
                    else
                    {
                        var prev = invariantSectors[idx - 1];
                        var next = invariantSectors[idx + 1];

                        if (prev.Length > next.Length)
                        {
                            prev.Stop = invariantSector.Stop;
                        }
                        else
                        {
                            next.Start = invariantSector.Start;
                        }
                    }
                }
            }
            
            return usefulSectors.ToArray();
        }

        /// <summary>
        /// Расчет периодичности для экваториального случая
        /// </summary>
        /// <param name="satellite"></param>
        /// <param name="inclination"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        PeriodicityViewResult CalculateEquator(Satellite satellite, Angle latitude)
        {
            var orbit = satellite.Orbit;
            var m = orbit.InputOrbitParameters.NCoil;
            var n = orbit.InputOrbitParameters.NDay;
            var inclination = orbit.InputOrbitParameters.InclinationAngle;
            ///Количество наблюдений
            var k = inclination.Rad > Math.PI / 2 ? m + n : m - n;
            ///Периодичность
            var periodicity = orbit.EraTier / k;
            ///Поток наблюдения
            var observationStream = new ObservationStream(CreateObservationMoments(k, periodicity), orbit.EraTier, periodicity);
            ///Участок инвариантности
            var invariantSector = new InvariantSector(new Angle(0), new Angle(orbit.DxNodeGrad), observationStream);
            ///Результат решения задачи анализа периодичности
            var periodicityViewResult = new PeriodicityViewResult(LatitudeType.Equator, latitude, invariantSector);
            return periodicityViewResult;
        }

        /// <summary>
        /// Расчет периодичности для полюса
        /// </summary>
        /// <param name="satellite"></param>
        /// <param name="inclination"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        PeriodicityViewResult CalculatePole(Satellite satellite, Angle latitude)
        {
            var orbit = satellite.Orbit;
            var m = orbit.InputOrbitParameters.NCoil;
            ///Количество наблюдений
            var k = m;
            ///Периодичность
            var periodicity = orbit.EraTier / k;
            ///Время полета спутника до вертекса орбиты (в данном случае до полюса)
            var timeToVertex = orbit.EraTurn / 4;
            ///Поток наблюдения
            var observationStream = new ObservationStream(CreateObservationMoments(k, periodicity, timeToVertex), orbit.EraTier, periodicity);
            ///Участок инвариантности
            var invariantSector = new InvariantSector(new Angle(0), new Angle(orbit.DxNodeGrad), observationStream);
            ///Результат решения задачи анализа периодичности
            var periodicityViewResult = new PeriodicityViewResult(LatitudeType.Pole, latitude, invariantSector);
            return periodicityViewResult;
        }

        /// <summary>
        /// Расчет моментов наблюдения по формулам из главы 2.4 методического пособия Ю.Н Разумного
        /// </summary>
        /// <param name="k"></param>
        /// <param name="orbit"></param>
        /// <param name=""></param>
        /// <param name="timeOffset"></param>
        /// <returns></returns>
        ObservationMoment[] CreateObservationMoments(int k, Orbit orbit, double timeOffset = 0)
        {
            var observationMoments = new List<ObservationMoment>();
            var nCoil = orbit.InputOrbitParameters.NCoil;
            var nDay = orbit.InputOrbitParameters.NDay;

            if (k != 0)
            {
                var observationMoment = new ObservationMoment(timeOffset);
                observationMoments.Add(observationMoment);
                for (int i = 2; i <= k; i++)
                {
                    var a = OM.VolutionNumber(nCoil, nDay, i);
                    ///Момент наблюдения
                    observationMoment = new ObservationMoment((a - 1) * orbit.EraTurn + timeOffset);
                    observationMoments.Add(observationMoment);
                }
                return observationMoments.OrderBy(t => t.T).ToArray();
            }
            else
            {
                return observationMoments.ToArray();
            }
        }

        /// <summary>
        /// Расчет моментов наблюдения для экватора и полюса по известной периодичности
        /// </summary>
        /// <param name="k"></param>
        /// <param name="periodicity"></param>
        /// <param name="timeOffset"></param>
        /// <returns></returns>
        ObservationMoment[] CreateObservationMoments(int k, double periodicity, double timeOffset = 0)
        {
            var observationMoments = new List<ObservationMoment>();
            for (int i = 0; i < k; i++)
            {
                ///Момент наблюдения
                var observationMoment = new ObservationMoment(i * periodicity + timeOffset);
                observationMoments.Add(observationMoment);
            }
            return observationMoments.ToArray();
        }

        /// <summary>
        /// Экваториальный случай (постоянное наблюдение)
        /// </summary>
        /// <param name="inclination"></param>
        /// <param name="band"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public bool IsEquatorCase(Angle inclination, Angle band, Angle latitude)
        {
            var inclination0 = Inclination0(inclination);
            return Math.Abs(latitude.Grad) + inclination0.Grad <= band.Grad;
        }

        /// <summary>
        /// Полюс
        /// </summary>
        /// <param name="inclination"></param>
        /// <param name="band"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public bool IsPoleCase(Angle inclination, Angle band, Angle latitude)
        {
            var i0 = Inclination0(inclination);
            var latitudeAbs = Math.Abs(latitude.Grad);
            return (i0.Grad - band.Grad <= latitudeAbs && latitudeAbs < i0.Grad + band.Grad && latitudeAbs >= 180 - (band.Grad + i0.Grad));
        }

        /// <summary>
        /// Преобразование наклонения в [0,90] диапазон
        /// </summary>
        /// <param name="inclination"></param>
        /// <returns></returns>
        Angle Inclination0(Angle inclination)
        {
            return new Angle(Math.Min(inclination.Grad, 180 - inclination.Grad));
        }

        /// <summary>
        /// Функция выделения целой части
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        int E(double a, double b)
        {
            return (int)Math.Truncate(a / b);
        }
    }
}
