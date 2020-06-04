using System;
using System.Drawing;

namespace MapEditor.Engine
{
    public interface IGraphics : IDisposable
    {
        int Width { get; }
        int Height { get; }

        void Clear();
        void DrawLines(Color color, Point[] points);
        void DrawRectangle(Brush brush, Rectangle area);
        void FillRectangle(Brush brush, Rectangle area);
        void DrawCircle(Color color, Rectangle area);
        void DrawImage(Image image, Rectangle area);

        void Render();
    }
}
