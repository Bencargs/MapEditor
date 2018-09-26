using System.Drawing;
using System.Windows.Forms;

namespace MapEditor.Engine
{
    public class WinFormGraphics : IGraphics
    {
        private readonly Graphics _graphics;
        private readonly Panel _window;

        public WinFormGraphics(Panel window)
        {
            _window = window;
            _graphics = window.CreateGraphics();
            Width = window.Width;
            Height = window.Height;
        }

        public int Width { get; }
        public int Height { get; }

        public void DrawLines(Color color, Point[] points)
        {
            var pen = new Pen(color, 1);
            _graphics.DrawLines(pen, points);
        }

        public void DrawImage(Image image, Rectangle area)
        {
            _graphics.DrawImageUnscaled(image, area);
        }

        //Tech debt
        public void FillRectangle(Brush brush, Rectangle area)
        {
            _graphics.FillRectangle(brush, area);
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            _window?.Dispose();
        }
    }
}
