using ATS.WPF.Shell.Helpers;
using ATS.WPF.Shell.Model;
using ATS.WPF.Shell.Screens;
using ATS.WPF.Shell.Shells;
using ATS.WPF.Shell.UI.AvalonMenuControl;
using Dissertation.Algorithms.Algorithms.Helpers;
using Dissertation.Modeling.Model.BallisticTasksComponents;
using Dissertation.Modeling.Modules.OrbitalEvaluatorModule;
using System;
using System.Windows;

namespace Dissertation.Modeling
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
    }
}
