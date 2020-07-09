using Autofac;
using Common;
using MapEngine.Commands;
using MapEngine.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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

            var messageHub = new MessageHub(container);

            var graphics = container.Resolve<WpfGraphics>();
            _scene = new Scene(graphics, messageHub,
                container.Resolve<MapHandler>(),
                container.Resolve<UnitHandler>(),
                container.Resolve<CameraHandler>());

            messageHub.Initialise();
            _scene.Initialise();

            frontBuffer.Source = graphics.Bitmap;

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += Update;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
            dispatcherTimer.Start();
        }

        private void Update(object sender, EventArgs e)
        {
            _scene.Display();
        }
    }
}
