namespace Common
{
    public interface IGraphics
    {
        int Width { get; }
        int Height { get; }

        void Clear();
        void DrawLines(Colour colour, Point[] points);
        void DrawRectangle(Colour colour, Rectangle area);
        void FillRectangle(Colour colour, Rectangle area);
        void DrawCircle(Colour colour, Rectangle area);
        void DrawImage(IImage image, Rectangle area);

        void Render();
    }
}
