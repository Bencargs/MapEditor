using Autofac;
using MapEngine.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MapEngine
{
    public static class RegistrationModule
    {
        public static IContainer Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(x => x.Name.EndsWith("Handler")) // there must be a better way..
                .SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
