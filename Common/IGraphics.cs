using System.Numerics;

namespace Common
{
    public interface IGraphics
    {
        int Width { get; }
        int Height { get; }

        void Clear();
        void DrawLine(Colour colour, Vector2 p1, Vector2 p2, int thickness = 1);
        void DrawRectangle(Colour colour, Rectangle area);
        void FillRectangle(Colour colour, Rectangle area);
        void DrawCircle(Colour colour, Rectangle area);
        void DrawImage(IImage image, Rectangle area);
        void DrawBytes(byte[] buffer, Rectangle area);
        void Desaturate(float[] buffer, Rectangle area);

        void Render();
    }
}
