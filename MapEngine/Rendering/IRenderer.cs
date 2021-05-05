using Common;

namespace MapEngine.Rendering
{
    public interface IRenderer
    {
        void DrawLayer(Rectangle viewport, IGraphics graphics);
    }
}
