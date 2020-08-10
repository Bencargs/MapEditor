﻿using Autofac;
using MapEngine.Commands;
using MapEngine.Handlers;

namespace MapEngine
{
    public static class RegistrationModule
    {
        public static IContainer Initialise()
        {
            var builder = new ContainerBuilder();

            var messageHub = RegisterMessageHub(builder);
            RegisterHandlers(builder, messageHub);
            RegisterServices(builder);

            var container = builder.Build();
            return container;
        }

        private static MessageHub RegisterMessageHub(ContainerBuilder builder)
        {
            var messageHub = new MessageHub();
            builder.RegisterInstance(messageHub);
            return messageHub;
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            var image = new WpfImage(640, 480);
            builder.RegisterInstance(new WpfGraphics(image)).SingleInstance();
        }

        private static void RegisterHandlers(ContainerBuilder builder, MessageHub messageHub)
        {
            var collisionHandler = new CollisionHandler();
            var weaponHandler = new WeaponHandler(messageHub, collisionHandler);
            var movementHandler = new MovementHandler(messageHub, collisionHandler);
            var unitHandler = new EntityHandler(messageHub, movementHandler, weaponHandler);
            
            builder.RegisterInstance(collisionHandler).SingleInstance();
            builder.RegisterInstance(movementHandler).SingleInstance();
            builder.RegisterInstance(weaponHandler).SingleInstance();
            builder.RegisterInstance(unitHandler).SingleInstance();
            builder.RegisterType<CameraHandler>().SingleInstance();
            builder.RegisterType<MapHandler>().SingleInstance();
        }
    }
}