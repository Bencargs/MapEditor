using Autofac;
using MapEngine.Commands;
using MapEngine.Handlers;
using System;
using System.Windows;
using System.Windows.Threading;
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
                container.Resolve<ParticleHandler>());

            messageHub.Initialise(container);
            _scene.Initialise();

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
