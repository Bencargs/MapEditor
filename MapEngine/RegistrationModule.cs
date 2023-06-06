using Autofac;
using MapEngine.Commands;
using MapEngine.Handlers;
using MapEngine.Handlers.ParticleHandler;
using MapEngine.Rendering;
using MapEngine.Services.Effect;
using MapEngine.Services.Effects.WaveEffect;
using MapEngine.Services.Map;
using MapEngine.Services.PathfindingService;

namespace MapEngine
{
    public static class RegistrationModule
    {
        public static IContainer Initialise()
        {
            var builder = new ContainerBuilder();

            RegisterMessageHub(builder);
            RegisterServices(builder);
            RegisterRenderers(builder);
            RegisterHandlers(builder);

            var container = builder.Build();
            return container;
        }

        private static void RegisterMessageHub(ContainerBuilder builder)
        {
            var messageHub = new MessageHub();
            builder.RegisterInstance(messageHub);
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            // todo: read window resolution from a config file
            //builder.RegisterInstance(new WpfGraphics(643, 428)).SingleInstance();
            builder.RegisterInstance(new WpfGraphics(768, 512)).SingleInstance();
            //builder.RegisterInstance(new WpfGraphics(640, 480)).SingleInstance();
            builder.RegisterType<MapService>().SingleInstance();
            builder.RegisterType<PathfindingService>().SingleInstance();
            builder.RegisterType<WaveEffectService>().SingleInstance();
            builder.RegisterType<FluidEffectService>().SingleInstance();
        }

        private static void RegisterHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<ParticleHandler>().SingleInstance();
            builder.RegisterType<CollisionHandler>().SingleInstance();
            builder.RegisterType<MovementHandler>().SingleInstance();
            builder.RegisterType<WeaponHandler>().SingleInstance();
            builder.RegisterType<EntityHandler>().SingleInstance();
            builder.RegisterType<CameraHandler>().SingleInstance();
            builder.RegisterType<EffectsHandler>().SingleInstance();
            builder.RegisterType<MapHandler>().SingleInstance();
            builder.RegisterType<SensorHandler>().SingleInstance();
        }

        private static void RegisterRenderers(ContainerBuilder builder)
        {
            builder.RegisterType<Renderer2d>().SingleInstance();
            builder.RegisterType<Renderer3d>().SingleInstance();
            builder.RegisterType<SensorRenderer>().SingleInstance();
        }
    }
}
