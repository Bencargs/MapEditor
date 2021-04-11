using Autofac;
using MapEngine.Commands;
using MapEngine.Handlers;
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
            builder.RegisterType<PathfindingService>().SingleInstance();
        }

        private static void RegisterHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<CollisionHandler>().SingleInstance();
            builder.RegisterType<MovementHandler>().SingleInstance();
            builder.RegisterType<WeaponHandler>().SingleInstance();
            builder.RegisterType<EntityHandler>().SingleInstance();
            builder.RegisterType<CameraHandler>().SingleInstance();
            builder.RegisterType<MapHandler>().SingleInstance();
            builder.RegisterType<SensorHandler>().SingleInstance();
        }
    }
}
