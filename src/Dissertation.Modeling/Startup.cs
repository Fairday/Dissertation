using ATS.Shell.Core;
using ATS.Shell.Providers;
using ATS.WPF.Shell.Bootstrap;
using ATS.WPF.Shell.Helpers;
using ATS.WPF.Shell.Screens;
using ATS.WPF.Shell.Services;
using ATS.WPF.Shell.Shells;
using ATS.WPF.Shell.UI.AvalonMenuControl;
using ATS.WPF.Shell.UI.Details;
using Autofac;
using Dissertation.Modeling.Modules.OrbitalEvaluatorModule;
using MaterialDesignThemes.Wpf;
using System.Linq;

namespace Dissertation.Modeling
{
    public class Startup : AutofacBootstrapperBase
    {
        public override void Configure(IContainer container, IServicesProvider servicesProvider)
        {
        }

        public override void OnStartup(IShell shell)
        {
            var themeService = (shell.ApplicationContainer as IContainer).Resolve<ThemeService>();
            (shell as Shell).IsDarkMode = false;
            var tealSwatch = themeService.Swatches.First(sw => sw.Name == "blue");
            themeService.ApplyAccentCommand.Execute(tealSwatch);
            themeService.ApplyPrimaryCommand.Execute(tealSwatch);
        }

        public override void ConfigureEntities(IContainer container, IEntitiesBinder binder)
        {
            binder.BindWithView<OrbitalSystemModule, OrbitalSystemScreen, OrbitalSystemView>();
        }

        public override void ConfigureModules(ContainerBuilder builder)
        {
            builder
                .RegisterType<OrbitalSystemModule>()
                .WithTitle("Орбитальная система")
                .WithIcon(PackIconKind.Application)
                .As<IModule>()
                .AsSelf()
                .SingleInstance();
        }

        public override void ConfigureScreens(ContainerBuilder builder)
        {
            builder
                .RegisterType<OrbitalSystemScreen>()
                .As<IScreen>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<MainAppScreen>()
                .As<IScreen>()
                .AsSelf()
                .SingleInstance();
        }

        public override void ConfigureServices(ContainerBuilder builder)
        {
        }

        public override void ConfigureViews(ContainerBuilder builder)
        {
            builder
                .RegisterType<OrbitalSystemView>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<AvalonMainWindow>()
                .As<IMainWindow>()
                .WithParameter("showMenu", false)
                .AsSelf()
                .SingleInstance();
        }
    }
}
