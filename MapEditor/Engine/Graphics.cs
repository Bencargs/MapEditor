using System.Drawing;
using System.Windows.Forms;

namespace MapEditor.Engine
{
    /// <summary>
    /// todo: rename to renderer?
    /// </summary>
    public class WinFormGraphics : IGraphics
    {
        private readonly Graphics _graphics;
        private readonly Panel _window;

        public WinFormGraphics(Panel window)
        {
            _window = window;
            Width = window.Width;
            Height = window.Height;

            _graphics = window.CreateGraphics();
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

        public void Render()
        {
            // todo: have all draw commands add to a back buffer
            // have render swap buffers
        }

        //Tech debt
        public void FillRectangle(Brush brush, Rectangle area)
        {
            _graphics.FillRectangle(brush, area);
        }

        public void DrawRectangle(Brush brush, Rectangle area)
        {
            _graphics.DrawRectangle(new Pen(brush), area);
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            _window?.Dispose();
        }
    }
}
