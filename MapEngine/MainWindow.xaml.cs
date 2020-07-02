using Common;
using MapEngine.Commands;
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
        private Scene _scene;

        public MainWindow()
        {
            InitializeComponent();

            var container = RegistrationModule.Init();
            var messageHub = new MessageHub(container);
            messageHub.Initialise();

            var image = new WpfImage(640, 480);
            frontBuffer.Source = image.Bitmap;

            var graphics = new WpfGraphics(image);
            _scene = new Scene(graphics);
            _scene.Initialise();

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += Update;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
            dispatcherTimer.Start();

            // Scene...
            //_graphics.Clear();

            //_graphics.Render();

            // graphics
            // scene
            // unit
            // camera
            // mouse (input handler)

            // load things

        }

        private void Update(object sender, EventArgs e)
        {
            _scene.Display();
        }
    }
}
