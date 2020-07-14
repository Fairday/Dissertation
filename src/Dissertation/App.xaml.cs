using OxyPlot;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System;
using ATS.WPF.Shell.Model;
using ATS.WPF.Shell.Shells;
using ATS.WPF.Shell.Screens;
using ATS.WPF.Shell.UI.AvalonMenuControl;
using ATS.WPF.Shell.Helpers;
using Dissertation.Modules.OrbitalModule;
using Dissertation.Algorithms.Model;
using Dissertation.Algorithms.Algorithms.Model;
using Dissertation.Algorithms.Metrica;
using Dissertation.Algorithms.Metrica.Visuals;

namespace Dissertation
{
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, arg) =>
            {
                try
                {
                    System.IO.File.WriteAllText("Crash.Exception.log", arg.ExceptionObject.ToString());
                }
                catch { }
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var shellConstructor = ShellConstructor.Create();
            var bootstrap = shellConstructor
                .Start<Shell, Startup, MultiActiveConductor>("PAG \"Orbital System Evaluator\"");

            shellConstructor.AddMenuModule<OrbitalSystemModule>();
            shellConstructor.AssignMainScreen<MainAppScreen, AvalonMainWindow, Shell>();

            var shellLifetime = shellConstructor.Complete();
            _ = ShellLifetimeManager.Launch(shellLifetime, bootstrap);
        }

        private void CreateMetrica()
        {
            var nGenerator = new DoubleRangeFeaturesPrecedentGenerator(1, 2, 1, "n");
            var mGenerator = new DoubleRangeFeaturesPrecedentGenerator(1, 33, 1, "m");
            var iGenerator = new DoubleRangeFeaturesPrecedentGenerator(-90, 90, 10, "i");
            var lattitudeGenerator = new DoubleRangeFeaturesPrecedentGenerator(-90, 90, 10, "lattitude");

            var nValues = nGenerator.Generate();
            var mValues = mGenerator.Generate();
            var iValues = iGenerator.Generate();
            var lattitudes = lattitudeGenerator.Generate();

            var precedents = MetricaExtensions.PreparePrecedents(nValues, mValues, iValues, lattitudes);

            var metrica = new SattelitePeriodicityViewMetrica(new SatelliteOld(0, 0, 1));
            metrica.EvaluateMetrica(precedents);

            var points1 = metrica.AverageEarchRadiusMRs.Select(mr => new DataPoint(mr.i, mr.AverageEarchRadius)).ToList();
            var points2 = metrica.AverageOrbitHeightMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.i).Select(gr => Tuple.Create(gr.Key,
                gr.Select(mr => new DataPoint(mr.l, mr.AverageOrbitHeight)).ToList()));
            var points3 = metrica.AoVMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.i).Select(gr => Tuple.Create(gr.Key,
                gr.Select(mr => new DataPoint(mr.l, mr.AngleOfView)).ToList()));
            var points4 = metrica.AWMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.i).Select(gr => Tuple.Create(gr.Key,
                gr.Select(mr => new DataPoint(mr.l, mr.AngleWidth)).ToList()));
            var points5 = metrica.LWMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.i).Select(gr => Tuple.Create(gr.Key,
                gr.Select(mr => new DataPoint(mr.l, mr.LinearWidth)).ToList()));
            var points6 = metrica.AlphaMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.Lattitude).Select(gr => Tuple.Create(gr.Key, gr.ToList().GroupBy(gr2 => gr2.i))).Select(gr => Tuple.Create(gr.Item1,
                gr.Item2.Select(gr2 => Tuple.Create(gr2.Key, gr2.Select(mr => new DataPoint(mr.l, mr.Alpha)).ToList()))));

            var results6 = new List<List<DataPoint>>();
            var titlesResults6 = new List<string>();

            foreach (var p1 in points6)
            {
                var currentFiPoints = new List<List<DataPoint>>();
                var currentTitles = new List<string>();

                foreach (var p2 in p1.Item2)
                {
                    results6.Add(p2.Item2);
                    titlesResults6.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentTitles.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentFiPoints.Add(p2.Item2);
                }

                //Current.Dispatcher.Invoke(() =>
                //{
                //    var gw = new GraphicWindow("alpha(fi, m, n, i)", currentFiPoints, "m/n", "alpha", "alpha(fi, m, n, i)", currentTitles.ToArray(), LegendPlacement.Outside);
                //    gw.Show();
                //});
            }

            var points7 = metrica.SMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.Lattitude).Select(gr => Tuple.Create(gr.Key, gr.ToList().GroupBy(gr2 => gr2.i))).Select(gr => Tuple.Create(gr.Item1,
                gr.Item2.Select(gr2 => Tuple.Create(gr2.Key, gr2.Select(mr => new DataPoint(mr.l, mr.S)).ToList()))));

            var results7 = new List<List<DataPoint>>();
            var titlesResults7 = new List<string>();

            foreach (var p1 in points7)
            {
                var currentFiPoints = new List<List<DataPoint>>();
                var currentTitles = new List<string>();

                foreach (var p2 in p1.Item2)
                {
                    results7.Add(p2.Item2);
                    titlesResults7.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentTitles.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentFiPoints.Add(p2.Item2);
                }

                //Current.Dispatcher.Invoke(() =>
                //{
                //    var gw = new GraphicWindow("s(fi, m, n, i)", currentFiPoints, "m/n", "s", "s(fi, m, n, i)", currentTitles.ToArray(), LegendPlacement.Outside);
                //    gw.Show();
                //});

            }

            var points8 = metrica.PVMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.Lattitude).Select(gr => Tuple.Create(gr.Key, gr.ToList().GroupBy(gr2 => gr2.i))).Select(gr => Tuple.Create(gr.Item1,
                gr.Item2.Select(gr2 => Tuple.Create(gr2.Key, gr2.Select(mr => new DataPoint(mr.l, mr.PeriodicityView)).ToList()))));

            var results8 = new List<List<DataPoint>>();
            var titlesResults8 = new List<string>();

            foreach (var p1 in points8)
            {
                var currentFiPoints = new List<List<DataPoint>>();
                var currentTitles = new List<string>();

                foreach (var p2 in p1.Item2)
                {
                    results8.Add(p2.Item2);
                    titlesResults8.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentTitles.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentFiPoints.Add(p2.Item2);
                }

                //Current.Dispatcher.Invoke(() =>
                //{
                //    var gw = new GraphicWindow("Tau(fi, m, n, i)", currentFiPoints, "m/n", "Tau", "Tau(fi, m, n, i)", currentTitles.ToArray(), LegendPlacement.Outside);
                //    gw.Show();
                //});
            }

            //Current.Dispatcher.Invoke(() =>
            //{
            //    var gw = new GraphicWindow("Rcp(i)", points1, "i", "Rcp");
            //    gw.Show();
            //});

            Current.Dispatcher.Invoke(() =>
            {
                var gw = new GraphicWindow("H(m, n, i)", points2.Select(p => p.Item2).ToList(), "m/n", "H", "H(m, n, i)", points2.Select(p => p.Item1.ToString()).ToArray());
                gw.Show();
            });

            //Current.Dispatcher.Invoke(() =>
            //{
            //    var gw = new GraphicWindow("Tetta(m, n, i)", points3.Select(p => p.Item2).ToList(), "m/n", "Tetta", "Tetta(m, n, i)", points3.Select(p => p.Item1.ToString()).ToArray());
            //    gw.Show();
            //});

            //Current.Dispatcher.Invoke(() =>
            //{
            //    var gw = new GraphicWindow("Betta(m, n, i)", points4.Select(p => p.Item2).ToList(), "m/n", "Betta", "Betta(m, n, i)", points4.Select(p => p.Item1.ToString()).ToArray());
            //    gw.Show();
            //});

            //Current.Dispatcher.Invoke(() =>
            //{
            //    var gw = new GraphicWindow("П(m, n, i)", points5.Select(p => p.Item2).ToList(), "m/n", "П", "П(m, n, i)", points5.Select(p => p.Item1.ToString()).ToArray());
            //    gw.Show();
            //});

            Current.Dispatcher.Invoke(() =>
            {
                var gw = new GraphicWindow("alpha(fi, m, n, i)", results6, "m/n", "alpha", "alpha(fi, m, n, i)", titlesResults6.ToArray(), LegendPlacement.Outside);
                gw.Show();
            });

            Current.Dispatcher.Invoke(() =>
            {
                var gw = new GraphicWindow("s(fi, m, n, i)", results7, "m/n", "s", "s(fi, m, n, i)", titlesResults7.ToArray(), LegendPlacement.Outside);
                gw.Show();
            });

            Current.Dispatcher.Invoke(() =>
            {
                var gw = new GraphicWindow("Tau(fi, m, n, i)", results8, "m/n", "Tau", "Tau(fi, m, n, i)", titlesResults8.ToArray(), LegendPlacement.Outside);
                gw.Show();
            });

            var points9 = metrica.PVMRs.OrderBy(mr => mr.l).ToList().GroupBy(mr => mr.Lattitude).Select(gr => Tuple.Create(gr.Key, gr.ToList().GroupBy(gr2 => gr2.i))).Select(gr => Tuple.Create(gr.Item1,
                gr.Item2.Select(gr2 => Tuple.Create(gr2.Key, gr2.Select(mr => new DataPoint(mr.l, mr.ObservationVariant.NodeSequential.Length)).ToList()))));

            var results9 = new List<List<DataPoint>>();
            var titlesResults9 = new List<string>();

            foreach (var p1 in points9)
            {
                var currentFiPoints = new List<List<DataPoint>>();
                var currentTitles = new List<string>();

                foreach (var p2 in p1.Item2)
                {
                    results9.Add(p2.Item2);
                    titlesResults9.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentTitles.Add("fi: " + p1.Item1.ToString() + " i: " + p2.Item1.ToString());
                    currentFiPoints.Add(p2.Item2);
                }

                //Current.Dispatcher.Invoke(() =>
                //{
                //    var gw = new GraphicWindow("Tau(fi, m, n, i)", currentFiPoints, "m/n", "Tau", "Tau(fi, m, n, i)", currentTitles.ToArray(), LegendPlacement.Outside);
                //    gw.Show();
                //});
            }

            Current.Dispatcher.Invoke(() =>
            {
                var gw = new GraphicWindow("Nodes(fi, m, n, i)", results9, "m/n", "Nodes", "Nodes(fi, m, n, i)", titlesResults9.ToArray(), LegendPlacement.Outside);
                gw.Show();
            });
        }
    }
}
