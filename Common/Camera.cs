using System.Numerics;

namespace Common
{
    public class Camera
    {
        public Rectangle Viewport { get; set; }
        public Rectangle InnerViewport => new Rectangle(Viewport.X + 20, Viewport.Y + 20, Viewport.Width - 40, Viewport.Height - 40);
        public Vector3 Location { get; set; }
    }
}
