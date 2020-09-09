using Autofac;
using MapEngine.Commands;
using MapEngine.Handlers;
using MapEngine.Services.Effects;
using MapEngine.Services.Map;
using MapEngine.Services.Navigation;

namespace MapEngine
{
    public static class RegistrationModule
    {
        public static IContainer Initialise()
        {
            var builder = new ContainerBuilder();

            RegisterMessageHub(builder);
            RegisterServices(builder);
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
            var image = new WpfImage(640, 480);
            builder.RegisterInstance(new WpfGraphics(image)).SingleInstance();
            builder.RegisterType<MapService>().SingleInstance();
            builder.RegisterType<WaveEffectService>().SingleInstance();
            builder.RegisterType<NavigationService>().SingleInstance();
        }

        private static void RegisterHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<CollisionHandler>().SingleInstance();
            builder.RegisterType<MovementHandler>().SingleInstance();
            builder.RegisterType<WeaponHandler>().SingleInstance();
            builder.RegisterType<EntityHandler>().SingleInstance();
            builder.RegisterType<CameraHandler>().SingleInstance();
            builder.RegisterType<EffectsHandler>().SingleInstance();
            builder.RegisterType<MapHandler>().SingleInstance();
        }
    }
}
