using ATS.MVVM.Collections;
using ATS.MVVM.Command;
using ATS.MVVM.Helpers;
using ATS.WPF.Modules.Helpers;
using ATS.WPF.Shell.Model;
using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Algorithms.OrbitalMath;
using Dissertation.Modeling.Algorithms;
using Dissertation.Modeling.Helpers;
using Dissertation.Modeling.Model;
using Dissertation.Modeling.Model.BallisticTasks;
using Dissertation.Modeling.Model.BallisticTasksComponents;
using Dissertation.Modeling.Model.Basics;
using Dissertation.Modeling.Model.EarchModel;
using Dissertation.Modeling.Model.Equipment;
using Dissertation.Modeling.Model.Extensions;
using Dissertation.Modeling.Model.OrbitalModel;
using Dissertation.Modeling.Model.OrbitalSystem;
using Dissertation.Modeling.Model.SatelliteModel;
using Dissertation.Modeling.UI;
using Dissertation.Modeling.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Dissertation.Modeling.Modules.OrbitalEvaluatorModule
{
    public class OrbitalSystemModule : ModuleBase
    {
        OrbitalSystemScreen _OrbitalSystemScreen;
        double dt;
        MoveModelingAlgorithm _MoveModelingAlgorithm;
        EarchLocationHalfSizingAlgorithm _EarchLocationHalfSizingAlgorithm;

        public ICanvasPointWrapper TopViewEarchPoint { get => Get(); set => Set(value); }
        public ICanvasPointWrapper TopViewSatellite { get => Get(); set => Set(value); }
        public ICanvasPointWrapper FrontViewEarchPoint { get => Get(); set => Set(value); }
        public ICanvasPointWrapper FrontViewSatellite { get => Get(); set => Set(value); }

        public ICommand StartCommand { get => Get(); set => Set(value); }
        public ICommand StartAnalyticCommand { get => Get(); set => Set(value); }
        public ICommand StartAnalysisTiersByModelingCommand { get => Get(); set => Set(value); }
        public ICommand StartAnalyticForCustomLocationCommand { get => Get(); set => Set(value); }
        public ICommand StartBatchAnalyticForCustomLocationCommand { get => Get(); set => Set(value); }
        public ICommand StartBatchAnalyticCommand { get => Get(); set => Set(value); }
        public ICommand StopCommand { get => Get(); set => Set(value); }
        public ICommand ClosePreviewObservationStream { get => Get(); set => Set(value); }
        public ICommand StartAnalysisTierCommand { get => Get(); set => Set(value); }
        public ICommand StartBatchAnalysisTierCommand { get => Get(); set => Set(value); }

        public ObservableList<ObservationStreamViewModel> ObservationStreams { get => Get(); set => Set(value); }
        public ObservableList<ObservationMomentViewModel> FirstCoilObservationMoments { get => Get(); set => Set(value); }
        public ObservationStreamViewModel SelectedObservationStream { get => Get(); set => Set(value); }

        public double CurrentTime { get => Get(); set => Set(value); }
        public double SatelliteLatitude { get => Get(); set => Set(value); }
        public double SatelliteLongitude { get => Get(); set => Set(value); }
        public double TraverseAngle { get => Get(); set => Set(value); }
        public double CosTraverseAngle { get => Get(); set => Set(value); }
        public double CosAzimuth { get => Get(); set => Set(value); }
        public double Azimuth { get => Get(); set => Set(value); }

        public InputOrbitParameters InputOrbitParameters { get => Get(); private set => Set(value); }

        public double SatelliteAscendingLongitude { get => Get(); set => Set(value); }
        public double SatelliteArgumentLatitude { get => Get(); set => Set(value); }
        public double EarchPointLatitude { get => Get(); set => Set(value); }
        public double StartEarchPointLongitude { get => Get(); set => Set(value); }
        public double StopEarchPointLongitude { get => Get(); set => Set(value); }
        public double StepEarchPointLongitude { get => Get(); set => Set(value); }

        public double Band { get => Get(); set => Set(value); }
        public bool Active { get => Get(); set => Set(value); }
        public bool ShowPointsCharting { get => Get(); set => Set(value); }
        public bool ShowCalculationResults { get => Get(); set => Set(value); }

        public bool X1 { get => Get(); set => Set(value); }
        public bool X2 { get => Get(); set => Set(value); }
        public bool X5 { get => Get(); set => Set(value); }
        public bool X10 { get => Get(); set => Set(value); }
        public bool X25 { get => Get(); set => Set(value); }
        public bool X100 { get => Get(); set => Set(value); }

        public Orbit Orbit { get => Get(); set => Set(value); }

        public OrbitalSystemModule()
        {
            ObservationStreams = new ObservableList<ObservationStreamViewModel>();
            FirstCoilObservationMoments = new ObservableList<ObservationMomentViewModel>();

            StartAnalysisTiersByModelingCommand = AsyncCommandCreator.Create(() =>
            {
                StartAnalysisTiersByModelig();
            });

            StartAnalyticForCustomLocationCommand = AsyncCommandCreator.Create(() =>
            {
                StartAnalyticForCustomLocation();
            });

            StartBatchAnalyticForCustomLocationCommand = AsyncCommandCreator.Create(() =>
            {
                StartBatchAnalyticForCustomLocation();
            });

            StartCommand = AsyncCommandCreator.CreateAsync((o) =>
            {
                return Start();
            }, (o) => !Active);

            StartAnalyticCommand = AsyncCommandCreator.CreateAsync((o) =>
            {
                return StartAnalytic();
            }, (o) => !Active);

            StartBatchAnalyticCommand = AsyncCommandCreator.CreateAsync((o) =>
            {
                return StartBatchAnalytic();
            }, (o) => !Active);

            StopCommand = new RelayCommand((o) =>
            {
               Stop();
            }, (o) => Active);

            ClosePreviewObservationStream = new RelayCommand((o) =>
            {
                SelectedObservationStream = null;
            });

            StartAnalysisTierCommand = AsyncCommandCreator.Create(() =>
            {
                StartAnalysisTier();
            });

            StartBatchAnalysisTierCommand = AsyncCommandCreator.CreateAsync((o) =>
            {
                return StartBatchAnalysisTier();
            }, (o) => !Active);
        }

        void StartAnalysisTier()
        {
            var orbit = OrbitalElementExtensions.CreateOrbit(12, 1, 50);

            var satellite1 = orbit.CreateSatellite(20, 0, 0);
            var satellite2 = orbit.CreateSatellite(20, 40, 100);
            var satellite3 = orbit.CreateSatellite(20, 60, 30);
            var satellite4 = orbit.CreateSatellite(20, 80, 120);

            var tier = (new Satellite[] { satellite1, satellite2, satellite3, satellite4 }).CreateTier();
            var task = new TierPeriodicityViewAnalyticBallisticTask(tier);
            var taskResult = task.CalculateAnalytic(new Angle(EarchPointLatitude));
        }

        Task StartBatchAnalysisTier() 
        {
            return Task.Run(() =>
            {
                Active = true;

                if (!Directory.Exists("results"))
                    Directory.CreateDirectory("results");

                var orbits = new Orbit[]
                {
                    OrbitalElementExtensions.CreateOrbit(2, 1, 5),
                    OrbitalElementExtensions.CreateOrbit(3, 1, 15),
                    OrbitalElementExtensions.CreateOrbit(4, 1, 45),
                    OrbitalElementExtensions.CreateOrbit(8, 1, 90),
                    OrbitalElementExtensions.CreateOrbit(9, 1, 180),
                    OrbitalElementExtensions.CreateOrbit(12, 1, 0),
                    OrbitalElementExtensions.CreateOrbit(14, 1, 70),
                    OrbitalElementExtensions.CreateOrbit(15, 1, 115),
                    OrbitalElementExtensions.CreateOrbit(27, 2, 15),
                    OrbitalElementExtensions.CreateOrbit(29, 2, 140),
                    OrbitalElementExtensions.CreateOrbit(63, 4, 60),
                };

                var directoryPath = Path.Combine("results", $"TierAnalysing_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm")}");
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                var output = new StreamWriter($"{directoryPath}/results.txt")
                {
                    AutoFlush = true,
                };

                var rnd = new Random();

                foreach (var orbit in orbits)
                {
                    output.WriteLine("***************************");
                    output.WriteLine(FormatOutput($"m={orbit.NCoil}", $"n={orbit.NDay}", $"i={orbit.InputOrbitParameters.Inclination}", $"band=20"));
                    for (int i = 2; i <= 4; i++)
                    {
                        var satellites = new Satellite[i];
                        int j = 0;
                        while (j < i) 
                            satellites[j++] = orbit.CreateSatellite(20, rnd.Next(10, 180), rnd.Next(10, 180));
                        var tier = satellites.CreateTier();
                        var task = new TierPeriodicityViewAnalyticBallisticTask(tier);

                        for (int fi = 0; fi <= 90; fi += 30) 
                        {
                            output.WriteLine("////////////////////");
                            output.WriteLine("Количество спутников:" + satellites.Length);
                            foreach (var s in satellites)
                                output.WriteLine($"Фазовое положение: {s.PhasePosition.LongitudeAscentNode.Grad}, {s.PhasePosition.LatitudeArgument.Grad}");
                            var taskResult = task.CalculateAnalytic(new Angle(fi));
                            if (taskResult == null) 
                            {
                                output.WriteLine("No observation");
                                continue;
                            }
                            foreach (var invariantSector in taskResult.InvariantSectors)
                            {
                                output.WriteLine(FormatOutput(($"length={invariantSector.Length.Grad}, amValid={invariantSector.Analytic_Modeling.IsValid}, aamValid={invariantSector.Accuracy_Analytic_Modeling.IsValid}, amo={invariantSector.Analytic_Modeling.HaveSimilarObservations}," +
                                    $" ame={invariantSector.Analytic_Modeling.MaximumRealTimeError}, aamo={invariantSector.Accuracy_Analytic_Modeling.HaveSimilarObservations}, aame={invariantSector.Accuracy_Analytic_Modeling.MaximumRealTimeError}, ve={invariantSector.Accuracy_Analytic_Modeling.MaximumValidTimeError}")));
                            }
                        }

                    }
                }

                Active = false;
            });
        }

        void StartAnalysisTiersByModelig()
        {
            var orbit1 = OrbitalElementExtensions.CreateOrbit(16, 1, 50);
            var orbit2 = OrbitalElementExtensions.CreateOrbit(2, 1, 30);
            var orbit3 = OrbitalElementExtensions.CreateOrbit(13, 1, 60);
            var orbit4 = OrbitalElementExtensions.CreateOrbit(8, 3, 70);

            var satellite1 = orbit1.CreateSatellite(10);
            var satellite2 = orbit2.CreateSatellite(15);
            var satellite3 = orbit3.CreateSatellite(12);
            var satellite4 = orbit4.CreateSatellite(8);

            var tier1 = satellite1.CreateTier();
            var tier2 = satellite2.CreateTier();
            var tier3 = satellite3.CreateTier();
            var tier4 = satellite4.CreateTier();

            var orbitalSystem = new OrbitalSystem(tier1, tier2, tier3, tier4);

            var task = new OrbitalSystemPeriodicityViewAnalyticBallisticTask(orbitalSystem);
            var taskResult = task.SolveWithModeling(new Angle(1), new Angle(0));
        }

       Task StartAnalytic()
        {
            return Task.Run(() =>
            {
                Active = true;
                Orbit.RecalculateOrbitCommand.Execute(null);
                var latitude = new Angle(EarchPointLatitude);
                var phasePosition = new PhasePosition(SatelliteAscendingLongitude, SatelliteArgumentLatitude);
                var beamEquipment = new BeamEquipment(new Angle(Band / 2));
                var satellite = new Satellite(Orbit, beamEquipment, phasePosition);
                var singleSatellitePeriodicityViewAnalyticBallisticTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(satellite);
                var result = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateAnalytic(latitude);

                if (!result.IsEmpty)
                {
                    foreach (var invariantSector in result.InvariantSectors)
                    {
                        var longitude = invariantSector.Center;
                        var earchLocation = new EarchLocation(latitude, longitude);
                        var observationStreamByModeling = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(Orbit, earchLocation,
                            phasePosition, _MoveModelingAlgorithm, beamEquipment.Band);
                        var observationStreamByAnalytic = invariantSector.ObservationStream;
                        var compareResult = ObservationStreamExtensions.CompareStreams(Orbit, observationStreamByAnalytic, observationStreamByModeling, Orbit.EraTurn / 4);
                        if (!compareResult.IsValid)
                        {
                        }
                        else
                        {
                        }
                    }
                }

                Active = false;
            });
        }

        Task StartBatchAnalyticForCustomLocation()
        {
            return Task.Run(() =>
            {
                Active = true;

                var dayCoilPairs = new List<DayCoilPair>()
                {
                    //new DayCoilPair(2, 1),
                    //new DayCoilPair(5, 1),
                    //new DayCoilPair(7, 1),
                    //new DayCoilPair(12, 1),
                    //new DayCoilPair(16, 1),
                    //new DayCoilPair(14, 1),
                    //new DayCoilPair(15, 2),
                    new DayCoilPair(27, 2),
                    //new DayCoilPair(29, 2),
                    new DayCoilPair(63, 4)
                };

                //var dayCoilPairs = new List<DayCoilPair>()
                //{
                //    new DayCoilPair(12, 1),
                //    new DayCoilPair(27, 2),
                //    new DayCoilPair(63, 4)
                //};

                if (!Directory.Exists("results"))
                    Directory.CreateDirectory("results");

                foreach (var dayCoilPair in dayCoilPairs)
                {

                    string directoryName = $"CL m={dayCoilPair.NCoil} n={dayCoilPair.NDay}";
                    var directoryPath = Path.Combine("results", directoryName);

                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    var crashes = new StreamWriter($"{directoryPath}/crashes.txt")
                    {
                        AutoFlush = true,
                    };
                    var invalidTimeAnalytic = new StreamWriter($"{directoryPath}/invalidTimeAnalytic.txt")
                    {
                        AutoFlush = true,
                    };
                    var invalidTimeAnalyticAccuracy = new StreamWriter($"{directoryPath}/invalidTimeAnalyticAccuracy.txt")
                    {
                        AutoFlush = true,
                    };
                    var invalidObservationsAnalytic = new StreamWriter($"{directoryPath}/invalidObservationsAnalytic.txt")
                    {
                        AutoFlush = true,
                    };
                    var invalidObservationsAnalyticAccuracy = new StreamWriter($"{directoryPath}/invalidObservationsAnalyticAccuracy.txt")
                    {
                        AutoFlush = true,
                    };

                    var count = 0;
                    for (int incl = 0; incl <= 180; incl += 20)
                    {
                        var orbit = new Orbit(new InputOrbitParameters(dayCoilPair.NCoil, dayCoilPair.NDay, incl));
                        var moveModelingAlgorithm = new MoveModelingAlgorithm(orbit);
                        for (int fi = 0; fi <= 90; fi += 10)
                        {
                            var latitude = new Angle(fi);

                            for (double band = 0.5; band <= 60.0; band += 5)
                            {
                                for (double ascNode = 0; ascNode <= 180; ascNode += 20)
                                {
                                    for (double latArg = 0; latArg <= 180; latArg += 20)
                                    {
                                        try
                                        {
                                            var phasePosition = new PhasePosition(ascNode, latArg);
                                            var beamEquipment = new BeamEquipment(new Angle(band));
                                            var satellite = new Satellite(orbit, beamEquipment, phasePosition);
                                            var singleSatellitePeriodicityViewAnalyticBallisticTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(satellite);
                                            var result = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateAnalytic(latitude);

                                            if (result.LatitudeType != LatitudeType.None)
                                            {
                                                for (double longt = 0; longt <= 180; longt += 20)
                                                {
                                                    try
                                                    {
                                                        var earchLocation = new EarchLocation(latitude, new Angle(longt));
                                                        singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateOffset(orbit,
                                                            phasePosition, earchLocation, out double timeoffset, out Angle longitude, out double anq);
                                                        ////Участок, в который попадает точка
                                                        var invariantSector = result.EntireInSector(longitude);

                                                        if ((invariantSector.Start + new Angle(0.2) >= longitude) || (invariantSector.Stop - new Angle(0.2)) <= longitude)
                                                        {
                                                            continue;
                                                        }

                                                        ////Уточненный поток наблюдения по участку инвариантности с аналитики
                                                        var observationStreamByModelingFromAnalytic = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(orbit,
                                                                    new EarchLocation(latitude, invariantSector.Center),
                                                                    new PhasePosition(0, 0), moveModelingAlgorithm, beamEquipment.Band);
                                                        ////Смещение аналитического потока наблюдения
                                                        var observationStreamByAnalytic = invariantSector.ObservationStream + timeoffset;
                                                        ////Смещение уточненного аналитического потока наблюдения
                                                        var shiftedObservationStreamByModelingFromAnalytic = observationStreamByModelingFromAnalytic + timeoffset;
                                                        ////Поток наблюдения, полученный с помощью моделирования
                                                        var observationStreamByModeling = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(orbit, earchLocation,
                                                                    phasePosition, moveModelingAlgorithm, beamEquipment.Band);
                                                        var compareResult_Analytic = ObservationStreamExtensions.CompareStreams(orbit, observationStreamByAnalytic, observationStreamByModeling, orbit.EraTurn / 4);
                                                        var compareResult_Accuracy_Analytic = ObservationStreamExtensions.CompareStreams(orbit, shiftedObservationStreamByModelingFromAnalytic, observationStreamByModeling, orbit.EraTurn / 4);

                                                        if (!compareResult_Analytic.IsValid)
                                                        {
                                                            var output_A = FormatOutput($"{result.LatitudeType}", $"m={dayCoilPair.NCoil}", $"n={dayCoilPair.NDay}", $"i={incl}", $"fi={fi}",
                                                                $"band={band}", $"ascNode={ascNode}", $"latArg={latArg}", $"longt={longt}", $"obserAnalytic={observationStreamByAnalytic.Count}",
                                                                $"observModel={observationStreamByModeling.Count}", $"long={Math.Round(longitude.Grad, 6)}",
                                                                $"error={compareResult_Analytic.MaximumRealTimeError}", $"limit={compareResult_Analytic.MaximumValidTimeError}", $"length={invariantSector.Length.Grad}");
                                                            var output_AA = FormatOutput($"{result.LatitudeType}", $"m={dayCoilPair.NCoil}", $"n={dayCoilPair.NDay}", $"i={incl}", $"fi={fi}",
                                                                $"band={band}", $"ascNode={ascNode}", $"latArg={latArg}", $"longt={longt}", $"obserAnalytic={shiftedObservationStreamByModelingFromAnalytic.Count}",
                                                                $"observModel={observationStreamByModeling.Count}", $"long={Math.Round(longitude.Grad, 6)}",
                                                                $"error={compareResult_Accuracy_Analytic.MaximumRealTimeError}", $"limit={compareResult_Accuracy_Analytic.MaximumValidTimeError}", $"length={invariantSector.Length.Grad}");
                                                            if (!compareResult_Analytic.HaveSimilarObservations)
                                                            {
                                                                invalidObservationsAnalytic.WriteLine(output_A);
                                                                invalidObservationsAnalytic.WriteLine(output_AA);
                                                                invalidObservationsAnalytic.WriteLine("---------------");
                                                            }
                                                            else
                                                            {
                                                                invalidTimeAnalytic.WriteLine(output_A);
                                                                invalidTimeAnalytic.WriteLine(output_AA);
                                                                invalidTimeAnalytic.WriteLine("---------------");
                                                            }
                                                        }
                                                        if (!compareResult_Accuracy_Analytic.IsValid)
                                                        {
                                                            var output_AA = FormatOutput($"{result.LatitudeType}", $"m={dayCoilPair.NCoil}", $"n={dayCoilPair.NDay}", $"i={incl}", $"fi={fi}",
                                                                $"band={band}", $"ascNode={ascNode}", $"latArg={latArg}", $"longt={longt}", $"obserAnalytic={shiftedObservationStreamByModelingFromAnalytic.Count}",
                                                                $"observModel={observationStreamByModeling.Count}", $"long={Math.Round(longitude.Grad, 6)}",
                                                                $"error={compareResult_Accuracy_Analytic.MaximumRealTimeError}", $"limit={compareResult_Accuracy_Analytic.MaximumValidTimeError}", $"length={invariantSector.Length.Grad}");
                                                            if (!compareResult_Accuracy_Analytic.HaveSimilarObservations)
                                                            {
                                                                invalidObservationsAnalyticAccuracy.WriteLine(output_AA);
                                                                invalidObservationsAnalyticAccuracy.WriteLine("---------------");
                                                            }
                                                            else
                                                            {
                                                                invalidTimeAnalyticAccuracy.WriteLine(output_AA);
                                                                invalidTimeAnalyticAccuracy.WriteLine("---------------");
                                                            }
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        var fo = FormatOutput($"m={dayCoilPair.NCoil}", $"n={dayCoilPair.NDay}", $"i={incl}", $"fi={fi}", $"band={band}", $"ascNode={ascNode}", $"latArg={latArg}", $"longt={longt}", $"{e.Message}");
                                                        crashes.WriteLine(fo);
                                                    }
                                                    finally
                                                    {
                                                        count++;
                                                        if (count % 10000 == 0)
                                                            Trace.WriteLine($"m={dayCoilPair.NCoil}, n={dayCoilPair.NDay},count={count}");
                                                    }
                                                }
                                            }

                                        }
                                        finally { }
                                    }
                                }
                            }
                        }
                    }
                }

                Active = false;
            });
        }

        Task StartAnalyticForCustomLocation()
        {
            return Task.Run(() =>
            {
                Active = true;

                Orbit.RecalculateOrbitCommand.Execute(null);
                var latitude = new Angle(EarchPointLatitude);
                var phasePosition = new PhasePosition(SatelliteAscendingLongitude, SatelliteArgumentLatitude);
                var beamEquipment = new BeamEquipment(new Angle(Band / 2));
                var satellite = new Satellite(Orbit, beamEquipment, phasePosition);
                var singleSatellitePeriodicityViewAnalyticBallisticTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(satellite);
                var result = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateAnalytic(latitude);

                var earchLocation = new EarchLocation(EarchPointLatitude, StartEarchPointLongitude);
                singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateOffset(Orbit,
                    phasePosition, earchLocation, out double timeoffset, out Angle longitude, out double ascNodeEquator);

                //временный поток
                //var tempModelingStream = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(Orbit, earchLocation,
                //            phasePosition, _MoveModelingAlgorithm, beamEquipment.Band);
                //var equatorModelingStream = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(Orbit, earchLocation,
                //            new PhasePosition(ascNodeEquator, 0, true), _MoveModelingAlgorithm, beamEquipment.Band);
                //var lngModelingStream = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(Orbit, new EarchLocation(EarchPointLatitude, longitude.Grad),
                //            new PhasePosition(0, 0, true), _MoveModelingAlgorithm, beamEquipment.Band);

                //Участок, в который попадает точка
                var invariantSector = result.EntireInSector(longitude);
                //Уточнный поток наблюдения по участку инвариантности с аналитики
                var observationStreamByModelingFromAnalytic = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(Orbit,
                            new EarchLocation(latitude, invariantSector.Center),
                            new PhasePosition(0, 0), _MoveModelingAlgorithm, beamEquipment.Band);
                //Смещение аналитического потока наблюдения
                var observationStreamByAnalytic = invariantSector.ObservationStream + timeoffset;
                //Смещение уточненного аналитического потока наблюдения
                var shiftedObservationStreamByModelingFromAnalytic = observationStreamByModelingFromAnalytic + timeoffset;
                //Поток наблюдения, полученный с помощью моделирования
                var observationStreamByModeling = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(Orbit, earchLocation,
                            phasePosition, _MoveModelingAlgorithm, beamEquipment.Band);
                var compareResult = ObservationStreamExtensions.CompareStreams(Orbit, observationStreamByAnalytic, observationStreamByModeling, Orbit.EraTurn / 4);
                var compareResult1 = ObservationStreamExtensions.CompareStreams(Orbit, shiftedObservationStreamByModelingFromAnalytic, observationStreamByModeling, Orbit.EraTurn / 4);

                Active = false;
            });
        }

        Task StartBatchAnalytic()
        {
            return Task.Run(() =>
            {
                var bts = new BoundedTaskScheduler(4);
                var taskFactory = new TaskFactory(bts);

                if (!Directory.Exists("results"))
                    Directory.CreateDirectory("results");

                //n
                Active = true;

                var phasePosition = new PhasePosition(0, 0);

                for (int n_g = 2; n_g <= 2; n_g++)
                {
                    for (int m_g = 27; m_g <= 27; m_g++)
                    {
                        var nod = MathEx.NOD(m_g, n_g);
                        if (nod == 1)
                        {
                            //Calculation
                            taskFactory.StartNew((args) =>
                            {
                                var count = 0;
                                var variables = (int[])args;
                                var m = variables[0];
                                var n = variables[1];

                                string directoryName = $"m={m} n={n}";
                                var directoryPath = Path.Combine("results", directoryName);

                                if (!Directory.Exists(directoryPath))
                                    Directory.CreateDirectory(directoryPath);

                                var valid = new StreamWriter($"{directoryPath}/valid.txt")
                                {
                                    AutoFlush = true,
                                };
                                var invalidTime = new StreamWriter($"{directoryPath}/invalidTime.txt")
                                {
                                    AutoFlush = true,
                                };
                                var noObservation = new StreamWriter($"{directoryPath}/noObs.txt")
                                {
                                    AutoFlush = true,
                                };
                                var crashes = new StreamWriter($"{directoryPath}/crashes.txt")
                                {
                                    AutoFlush = true,
                                };
                                var invalidMomentsAnalytic = new StreamWriter($"{directoryPath}/invalidMomentsAnalytic.txt");
                                var invalidMomentsModel = new StreamWriter($"{directoryPath}/invalidMomentsModel.txt")

                                {
                                    AutoFlush = true,
                                };

                                for (int incl = 0; incl <= 180; incl++)
                                {
                                    var orbit = new Orbit(new InputOrbitParameters(m, n, incl));
                                    var moveModelingAlgorithm = new MoveModelingAlgorithm(orbit);
                                    //fi
                                    var sw = new Stopwatch();
                                    sw.Start();

                                    for (int fi = 0; fi <= 90; fi++)
                                    {
                                        var latitude = new Angle(fi);
                                        //band
                                        for (double band = 0.5; band <= 60.0; band += 0.5)
                                        {
                                            try
                                            {
                                                var beamEquipment = new BeamEquipment(new Angle(band));
                                                var satellite = new Satellite(orbit, beamEquipment, phasePosition);
                                                var singleSatellitePeriodicityViewAnalyticBallisticTask = new SingleSatellitePeriodicityViewAnalyticBallisticTask(satellite);
                                                var result = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateAnalytic(latitude);

                                                if (result.IsEmpty)
                                                {
                                                    var fo = FormatOutput($"{result.LatitudeType}", $"m={m}", $"n={n}", $"i={incl}", $"fi={fi}", $"band={band}", "no observation");
                                                    //noObservation.WriteLine(fo);
                                                }
                                                else
                                                {
                                                    foreach (var invariantSector in result.InvariantSectors)
                                                    {
                                                        var longitude = invariantSector.Center;
                                                        var earchLocation = new EarchLocation(latitude, longitude);
                                                        var observationStreamByModeling = singleSatellitePeriodicityViewAnalyticBallisticTask.CalculateModeling(orbit, earchLocation,
                                                            phasePosition, moveModelingAlgorithm, beamEquipment.Band);
                                                        var observationStreamByAnalytic = invariantSector.ObservationStream;
                                                        var compareResult = ObservationStreamExtensions.CompareStreams(orbit, observationStreamByAnalytic, observationStreamByModeling, orbit.EraTurn / 3);
                                                        if (!compareResult.IsValid)
                                                        {
                                                            var output = FormatOutput($"{result.LatitudeType}", $"m={m}", $"n={n}", $"i={incl}", $"fi={fi}",
                                                                $"band={band}", $"obserAnalytic={observationStreamByAnalytic.Count}",
                                                                $"observModel={observationStreamByModeling.Count}", $"long={Math.Round(longitude.Grad, 6)}", $"valid={compareResult.IsValid}",
                                                                $"error={compareResult.MaximumRealTimeError}", $"limit={compareResult.MaximumValidTimeError}", $"length={invariantSector.Length.Grad}");
                                                            //Trace.WriteLine(output);
                                                            if (!compareResult.HaveSimilarObservations)
                                                            {
                                                                if (compareResult.AnalyticMore)
                                                                    invalidMomentsAnalytic.WriteLine(output);
                                                                else
                                                                    invalidMomentsModel.WriteLine(output);
                                                            }
                                                            else
                                                                invalidTime.WriteLine(output);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                var fo = FormatOutput($"m={m}", $"n={n}", $"i={incl}", $"fi={fi}", $"band={band}", $"{e.Message}");
                                                crashes.WriteLine(fo);
                                            }
                                            finally
                                            {
                                                count++;
                                                if (count % 10000 == 0)
                                                    Trace.WriteLine($"m={m}, n={n},count={count}");
                                            }
                                        }
                                        GC.Collect();
                                    }

                                    sw.Stop();
                                    Trace.WriteLine($"i={incl}: {sw.Elapsed}");
                                }

                                valid.Dispose();
                                invalidTime.Dispose();
                                invalidMomentsAnalytic.Dispose();
                                invalidMomentsModel.Dispose();
                                noObservation.Dispose();
                                crashes.Dispose();
                            }, (object)(new int[] { m_g , n_g}));
                        }
                        else
                        {

                        }
                    }
                }

                Active = false;
            });
        }

        //TODO: Убрать Acos
        Task Start()
        {
            return Task.Run(async () =>
            {
                Active = true;
                Orbit.RecalculateOrbitCommand.Execute(null);
                var halfBandGrad = Band / 2;
                var halfBandRad = Band.ToRad() / 2;

                var nomalizedStart = RangeNormalizer.DPI.Normalize(StartEarchPointLongitude.ToRad(), true).ToGrad();
                var normalizedStop = RangeNormalizer.DPI.Normalize(StopEarchPointLongitude.ToRad(), true).ToGrad();
                var normalizedStep = RangeNormalizer.DPI.Normalize(StepEarchPointLongitude.ToRad(), true).ToGrad();

                var start = StartEarchPointLongitude;
                var stop = StopEarchPointLongitude < StartEarchPointLongitude ? StopEarchPointLongitude + 360 : StopEarchPointLongitude;
                var current = start;
                var normalizedCurrent = nomalizedStart;

                bool singlePoint = false;
                if (normalizedStep <= 0)
                    singlePoint = true;

                UIHelper.Dispatcher.Invoke(() =>
                {
                    ObservationStreams.Clear();
                    FirstCoilObservationMoments.Clear();
                });

                var averageEarthRadius = OM.AverageEarchRadius(Orbit.InputOrbitParameters.Inclination);
                var toCanvasLayoutNormalizer = new FullRangeNormalizer(0, averageEarthRadius, 0, 175);

                while (Active && (current <= stop + MathConstants.SlipAngleGRad || singlePoint))
                {
                    CurrentTime = 0;
                    var startSatellitePosition = new PhasePosition(SatelliteAscendingLongitude, SatelliteArgumentLatitude);

                    var earchLocation = new EarchLocation(EarchPointLatitude, normalizedCurrent);
                    var observationStream = new ObservationStreamViewModel(earchLocation);


                    if (ShowCalculationResults)
                    {
                        UIHelper.Dispatcher.Invoke(() =>
                        {
                            ObservationStreams.Add(observationStream);
                        });
                    }

                    var earchPointTrace = new EarchPointTrace(new Angle(Orbit.InputOrbitParameters.Inclination), earchLocation);
                    var satelliteTrace = new SatelliteTrace(Orbit, startSatellitePosition);
                    _EarchLocationHalfSizingAlgorithm = new EarchLocationHalfSizingAlgorithm(
                        _MoveModelingAlgorithm,
                        new Angle(Orbit.InputOrbitParameters.Inclination),
                        AccuracyModel.CalculationAccuracy,
                        startSatellitePosition,
                        earchPointTrace);

                    int? cosAngleSign = null;
                    bool isSignChanged = false;
                    double calculationDeltaTimeTail = 0;

                    if (AngleExtensions.Sum(
                        earchPointTrace.Location.Longitude,
                        satelliteTrace.Position.LatitudeArgument,
                        satelliteTrace.Position.LongitudeAscentNode) == 0)
                    {
                        if (Orbit.InputOrbitParameters.InclinationAngle.Rad == MathConstants.HPI)
                            calculationDeltaTimeTail = 0;
                        else
                        {
                            calculationDeltaTimeTail = 0;
                            if (Orbit.InputOrbitParameters.InclinationAngle.Rad < MathConstants.HPI)
                                cosAngleSign = 1;
                        }
                    }
                    else
                    {
                        if (Orbit.InputOrbitParameters.InclinationAngle.Rad > MathConstants.HPI)
                            calculationDeltaTimeTail = dt;
                        else
                            calculationDeltaTimeTail = 0;
                    }

                    do
                    {
                        earchPointTrace.Calculate(CurrentTime);
                        satelliteTrace.Calculate(CurrentTime);

                        CosTraverseAngle = CoordinateSystemConverter.CosBy(earchPointTrace.Location, satelliteTrace.LocationBySpeed);
                        TraverseAngle = Math.Acos(CosTraverseAngle);
                        CosAzimuth = CoordinateSystemConverter.CosBy(earchPointTrace.Location, satelliteTrace.LocationByCoord);
                        Azimuth = Math.Acos(CosAzimuth);
                        var newSign = Math.Sign(CosTraverseAngle);
                        if (newSign != cosAngleSign)
                            isSignChanged = true;
                        cosAngleSign = newSign;

                        if (isSignChanged)
                        {
                            double previousTime = CurrentTime - dt;
                            var computedMoment = _EarchLocationHalfSizingAlgorithm.Compute(previousTime, CurrentTime);

                            satelliteTrace.CalculatePhasePosition(computedMoment.Parameter);
                            satelliteTrace.Calculate(computedMoment.Parameter);
                            earchPointTrace.Calculate(computedMoment.Parameter);
                            CosAzimuth = CoordinateSystemConverter.CosBy(earchPointTrace.Location, satelliteTrace.LocationByCoord);

                            if (CosAzimuth > Math.Cos(halfBandRad + AccuracyModel.AngleAccuracyRad))
                            {
                                if (Math.Abs(computedMoment.Value) < AccuracyModel.CalculationAccuracy)
                                {
                                    UIHelper.Dispatcher.Invoke(() =>
                                    {
                                        if (observationStream.ObservationMoments.Count > 0)
                                        {
                                            observationStream.ObservationMoments.Add(new ObservationMomentViewModel(
                                                computedMoment.Parameter,
                                                computedMoment.Parameter - observationStream.ObservationMoments.Last().TSource)
                                            {
                                                EarchLocation = satelliteTrace.LocationByCoord,
                                            });
                                        }
                                        else
                                        {
                                            observationStream.ObservationMoments.Add(new ObservationMomentViewModel(computedMoment.Parameter, 0)
                                            {
                                                EarchLocation = satelliteTrace.LocationByCoord,
                                            });
                                        }

                                        if (computedMoment.Parameter <= Orbit.EraTurn)
                                        {
                                            FirstCoilObservationMoments.Add(new ObservationMomentViewModel(computedMoment.Parameter, 0)
                                            {
                                                EarchLocation = earchPointTrace.Location,
                                                SatelliteEarchLocation = satelliteTrace.LocationByCoord,
                                            });
                                        }
                                    });
                                }
                            }
                            isSignChanged = false;
                        }

                        //Широта спутника
                        SatelliteLatitude = Math.Round(satelliteTrace.LocationByCoord.Latitude.Grad, 3);
                        //Долгота спутника
                        SatelliteLongitude = Math.Round(satelliteTrace.LocationByCoord.Longitude.Grad, 3);

                        MovePointsOnUI(toCanvasLayoutNormalizer, satelliteTrace, earchPointTrace);

                        CurrentTime += dt;
                        satelliteTrace.CalculatePhasePosition(CurrentTime);

                        if (ShowPointsCharting)
                            await Task.Delay(5).ConfigureAwait(false);
                    }
                    while (Active && CurrentTime < Orbit.EraTier + calculationDeltaTimeTail);

                    if (observationStream.ObservationMoments.Count > 1)
                    {
                        observationStream.ObservationMoments[0].DeltaWithPrevious = (Orbit.EraTier - observationStream.ObservationMoments[observationStream.ObservationMoments.Count - 1].TSource) + observationStream.ObservationMoments[0].TSource;
                    }
                    else if (observationStream.ObservationMoments.Count == 1)
                    {
                        observationStream.ObservationMoments[0].DeltaWithPrevious = Orbit.EraTier;
                    }

                    observationStream.Periodicity = PeriodicityViewEvaluator.CalculateFast(observationStream.ObservationMoments);

                    if (singlePoint)
                        break;

                    normalizedCurrent = RangeNormalizer.DPI.Normalize((normalizedCurrent + normalizedStep).ToRad(), true).ToGrad();
                    current += normalizedStep;
                }

              Active = false;
            });
        }

        void MovePointsOnUI(FullRangeNormalizer toCanvasLayoutNormalizer, SatelliteTrace satelliteTrace, EarchPointTrace earchPointTrace)
        {
            if (ShowPointsCharting)
            {
                //Дальше построение графиков
                var S_normalizedX = toCanvasLayoutNormalizer.Normalize(satelliteTrace.PointXYZ.X);
                var S_normalizedY = toCanvasLayoutNormalizer.Normalize(satelliteTrace.PointXYZ.Y);
                var S_normalizedZ = toCanvasLayoutNormalizer.Normalize(satelliteTrace.PointXYZ.Z);
                TopViewSatellite.SetCoordinateLocation(Direction.Top, new Vector(-S_normalizedX, S_normalizedY, S_normalizedZ), true);
                FrontViewSatellite.SetCoordinateLocation(Direction.Front, new Vector(-S_normalizedX, S_normalizedY, S_normalizedZ), true);

                var P_normalizedX = toCanvasLayoutNormalizer.Normalize(earchPointTrace.PointXYZ.X);
                var P_normalizedY = toCanvasLayoutNormalizer.Normalize(earchPointTrace.PointXYZ.Y);
                var P_normalizedZ = toCanvasLayoutNormalizer.Normalize(earchPointTrace.PointXYZ.Z);
                TopViewEarchPoint.SetCoordinateLocation(Direction.Top, new Vector(-P_normalizedX, P_normalizedY, P_normalizedZ), true);
                FrontViewEarchPoint.SetCoordinateLocation(Direction.Front, new Vector(-P_normalizedX, P_normalizedY, P_normalizedZ), true);
            }
        }

        void Stop()
        {
            Active = false;
        }

        public override void Initialized()
        {
            _OrbitalSystemScreen = BoundedScreen as OrbitalSystemScreen;

            TopViewSatellite = _OrbitalSystemScreen.CreatePoint(ViewType.Top, Brushes.Green);
            TopViewEarchPoint = _OrbitalSystemScreen.CreatePoint(ViewType.Top, Brushes.Red);

            FrontViewSatellite = _OrbitalSystemScreen.CreatePoint(ViewType.Front, Brushes.Green);
            FrontViewEarchPoint =_OrbitalSystemScreen.CreatePoint(ViewType.Front, Brushes.Red);

            InputOrbitParameters = new InputOrbitParameters(16, 1, 0);
            Orbit = new Orbit(InputOrbitParameters);

            X1 = true;
            dt = 1;
            Band = 17.9671;
            _MoveModelingAlgorithm = new MoveModelingAlgorithm(Orbit);

            this.ToFluent().Bind(o => o.X1).OnSet((oV, nV) =>
            {
                if (nV)
                {
                    dt = 1;
                }
                return nV;
            });

            this.ToFluent().Bind(o => o.X2).OnSet((oV, nV) =>
            {
                if (nV)
                {
                    dt = 2;
                }
                return nV;
            });

            this.ToFluent().Bind(o => o.X5).OnSet((oV, nV) =>
            {
                if (nV)
                {
                    dt = 5;
                }
                return nV;
            });

            this.ToFluent().Bind(o => o.X10).OnSet((oV, nV) =>
            {
                if (nV)
                {
                    dt = 10;
                }
                return nV;
            });

            this.ToFluent().Bind(o => o.X25).OnSet((oV, nV) =>
            {
                if (nV)
                {
                    dt = 25;
                }
                return nV;
            });

            this.ToFluent().Bind(o => o.X100).OnSet((oV, nV) =>
            {
                if (nV)
                {
                    dt = 100;
                }
                return nV;
            });
        }

        string FormatOutput(params string[] args)
        {
            int size = 5;
            string summary = string.Empty;

            foreach (var arg in args)
            {
                var missingLength = size - arg.Length - 1;
                if (missingLength < 0)
                {
                    missingLength = 5;
                }
                var part = arg + "," + new string(' ', missingLength);
                summary += part;
            }

            return summary;
        }
    }
}
