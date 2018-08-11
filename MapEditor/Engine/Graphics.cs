using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapEditor
{
    public interface IGraphics : IDisposable
    {
        int Width { get; }
        int Height { get; }

        void DrawLines(Color color, Point[] points);
        void FillRectangle(Brush brush, Rectangle area);
        void DrawImage(Image image, Point point);
    }

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

        public void DrawImage(Image image, Point point)
        {
            _graphics.DrawImageUnscaled(image, point);
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
