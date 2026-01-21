using Autofac;
using Common;
using MapEngine.Commands;
using MapEngine.Handlers;
using MapEngine.Handlers.InputHandler;
using MapEngine.Handlers.ParticleHandler;
using MapEngine.Handlers.SensorHandler;
using MapEngine.Rendering;
using MapEngine.Services.Effect;
using MapEngine.Services.Effects.LightingEffect;
using MapEngine.Services.Effects.WaveEffect;
using MapEngine.Services.Map;
using MapEngine.Services.PathfindingService;
using IContainer = Autofac.IContainer;

namespace MapEngine
{
    public static class RegistrationModule
    {
        // todo: replace all this with a reflective type loader?

        public static IContainer Initialise()
        {
            var builder = new ContainerBuilder();

            RegisterMessageHub(builder);
            RegisterModels(builder);
            RegisterServices(builder);
            RegisterRenderers(builder);
            RegisterHandlers(builder);
            RegisterCommands(builder);

            var container = builder.Build();
            return container;
        }

        private static void RegisterMessageHub(ContainerBuilder builder)
        {
            var messageHub = new MessageHub();
            builder.RegisterInstance(messageHub);
        }

        private static void RegisterModels(ContainerBuilder builder)
        {
            builder.RegisterType<Minimap>().SingleInstance();
            builder.RegisterType<GameTime>().SingleInstance();
            builder.RegisterType<InputState>().SingleInstance();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            // todo: read window resolution from a config file
            //builder.RegisterInstance(new WpfGraphics(643, 428)).SingleInstance();
            //builder.RegisterInstance(new WpfGraphics(768, 512)).SingleInstance();
            builder.RegisterInstance(new WpfGraphics(1779, 743)).SingleInstance();
            //builder.RegisterInstance(new WpfGraphics(640, 480)).SingleInstance();
            builder.RegisterType<MapService>().SingleInstance();
            builder.RegisterType<PathfindingService>().SingleInstance();
            builder.RegisterType<WaveEffectService>().SingleInstance();
            builder.RegisterType<FluidEffectService>().SingleInstance();
            builder.RegisterType<ShadowEffectService>().SingleInstance();
            builder.RegisterType<LightingEffectService>().SingleInstance();
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
            RegisterSensorHandler(builder);
            builder.RegisterType<InputHandler>().SingleInstance();
            builder.RegisterType<InterfaceHandler>().SingleInstance();
            builder.RegisterType<CargoHandler>().SingleInstance();
            builder.RegisterType<CursorHandler>().SingleInstance();
            builder.RegisterType<TextHandler>().SingleInstance();
            builder.RegisterType<MinimapHandler>().SingleInstance();
        }

        private static void RegisterRenderers(ContainerBuilder builder)
        {
            builder.RegisterType<Renderer2d>().SingleInstance();
            builder.RegisterType<Renderer3d>().SingleInstance();
        }

        private static void RegisterSensorHandler(ContainerBuilder builder)
        {
            builder.RegisterType<SensorHandler>().SingleInstance();
            builder.RegisterType<RadarSensor>().SingleInstance();
            builder.RegisterType<SightSensor>().SingleInstance();
        }

        private static void RegisterCommands(ContainerBuilder builder)
        {
            builder.RegisterType<MoveCommandStrategy>();
            builder.RegisterType<UnloadCommandStrategy>();
        }
    }
}
