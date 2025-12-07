using Autofac;
using MapEngine.Commands;
using MapEngine.Handlers;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Remoting.Channels;
using System.Windows;
using System.Windows.Threading;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.Handlers.ParticleHandler;

namespace MapEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Scene _scene;

        public MainWindow()
        {
            InitializeComponent();

            var container = RegistrationModule.Initialise();

            var messageHub = container.Resolve<MessageHub>();
            var graphics = container.Resolve<WpfGraphics>();
            _scene = new Scene(graphics, messageHub,
                container.Resolve<MapHandler>(),
                container.Resolve<EntityHandler>(),
                container.Resolve<CameraHandler>(),
                container.Resolve<EffectsHandler>(),
                container.Resolve<ParticleHandler>(),
                container.Resolve<InterfaceHandler>());

            messageHub.Initialise(container);
            _scene.Initialise();

            var inputHandler = container.Resolve<InputHandler>();
            MouseLeftButtonDown += (sender, args) =>
            {
                var location = inputHandler.GetMouseLocation(args, frontBuffer);
                inputHandler.HandleLeftMouseDown(location);
            };
            MouseLeftButtonUp += (sender, args) =>
            {
                var location = inputHandler.GetMouseLocation(args, frontBuffer);
                inputHandler.HandleLeftMouseUp(location);
            };
            MouseRightButtonDown += (sender, args) =>
            {
                var location = inputHandler.GetMouseLocation(args, frontBuffer);
                inputHandler.HandleRightMouseDown(location);
            };
            MouseMove += (sender, args) =>
            {
                var location = inputHandler.GetMouseLocation(args, frontBuffer);
                inputHandler.HandleMouseMove(location);
            };
            KeyDown += (sender, args) =>
            {
                inputHandler.HandleKeyDown(args.Key);
            };

            frontBuffer.Source = graphics.Bitmap;

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += Update;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20); // should be 16 for 60fps
            dispatcherTimer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            _scene.Display();
        }
    }
}
