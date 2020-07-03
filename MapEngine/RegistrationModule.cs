using Autofac;
using System.Linq;
using System.Reflection;

namespace MapEngine
{
    public static class RegistrationModule
    {
        public static IContainer Initialise()
        {
            var builder = new ContainerBuilder();

            RegisterServices(builder);
            RegisterHandlers(builder);

            var container = builder.Build();
            return container;
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            var image = new WpfImage(640, 480);
            builder.RegisterInstance(new WpfGraphics(image)).SingleInstance();
        }

        private static void RegisterHandlers(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(x => x.Name.EndsWith("Handler")) // there must be a better way..
                .SingleInstance();
        }
    }
}
