using System;
using System.Drawing;

namespace MapEditor.Engine
{
    public interface IGraphics : IDisposable
    {
        int Width { get; }
        int Height { get; }

        void DrawLines(Color color, Point[] points);
        void FillRectangle(Brush brush, Rectangle area);
        void DrawImage(Image image, Rectangle area);

        void Render();
    }
}
