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
using Dissertation.Adapters;
using Dissertation.Algorithms.Model;
using Dissertation.Basics.Mapping;
using Dissertation.Model;
using Dissertation.Modules.OrbitalModule;
using Dissertation.Modules.OrbitalModule.MeasurementModule;
using MaterialDesignThemes.Wpf;
using ProcessingModule.Common;
using System.Linq;

namespace Dissertation
{
    public class Startup : AutofacBootstrapperBase
    {
        public override void OnStartup(IShell shell)
        {
            var themeService = (shell.ApplicationContainer as IContainer).Resolve<ThemeService>();
            (shell as Shell).IsDarkMode = true;
            var tealSwatch = themeService.Swatches.First(sw => sw.Name == "blue");
            themeService.ApplyAccentCommand.Execute(tealSwatch);
            themeService.ApplyPrimaryCommand.Execute(tealSwatch);
        }

        public override void Configure(IContainer container, IServicesProvider servicesProvider)
        {
            ConfigureAdapters(container, container.Resolve<IMapper>());
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
            RegisterAdapters(builder);
            RegisterMapper(builder);
            RegisterOrbitalSystemProcessor(builder);
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

        private void RegisterOrbitalSystemProcessor(ContainerBuilder builder)
        {
            builder.RegisterSingletonBy<MeasuringLogger, IProcessLogger>();
            builder.RegisterSingleton<OrbitalSystemMeasurementContext>();
            builder.RegisterSingletonBy<OrbitalSystemMeasurementProcessor, IMeasuringController<OrbitalSystemMeasurementContext>>();
            builder.RegisterSingletonBy<OrbitalSystemMeasurementManager, MeasuringProcessManagerBase<OrbitalSystemMeasurementContext>>();
        }

        private void RegisterMapper(ContainerBuilder builder)
        {
            builder.RegisterSingletonBy<Mapper, IMapper>();
        }

        private void RegisterAdapters(ContainerBuilder builder)
        {
            builder.RegisterSingletonBy<SatelliteAdapter, IAdapter<SatelliteModel, SatelliteOld>>();
            builder.RegisterSingletonBy<LatutideAdapter, IAdapter<LatitudeModel, Latitude>>();
            builder.RegisterSingletonBy<OrbitInfoAdapter, IAdapter<OrbitInfo, OrbitParameters>>();
        }

        private void ConfigureAdapters(IContainer container, IMapper mapper)
        {
            mapper.RegisterAdapter<SatelliteModel, SatelliteOld, SatelliteAdapter>(container.Resolve<SatelliteAdapter>());
            mapper.RegisterAdapter<LatitudeModel, Latitude, LatutideAdapter>(container.Resolve<LatutideAdapter>());
            mapper.RegisterAdapter<OrbitInfo, OrbitParameters, OrbitInfoAdapter>(container.Resolve<OrbitInfoAdapter>());
        }
    }
}
