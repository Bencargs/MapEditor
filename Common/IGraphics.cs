using System.Numerics;

namespace Common
{
    public interface IGraphics
    {
        int Width { get; }
        int Height { get; }

        void Clear();
        void DrawLines(Colour colour, Vector2[] points);
        void DrawRectangle(Colour colour, Rectangle area);
        void FillRectangle(Colour colour, Rectangle area);
        void DrawCircle(Colour colour, Rectangle area);
        void DrawImage(IImage image, Rectangle area);
        void DrawBytes(byte[] buffer, Rectangle area);
        void Apply(byte[] buffer);

        void Render();
    }
}
