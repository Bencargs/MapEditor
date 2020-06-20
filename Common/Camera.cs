using System.Numerics;

namespace Common
{
    public class Camera
    {
        public Vector3 Location { get; set; }
        public Rectangle Viewport { get; set; }
        public Rectangle InnerViewport { get; set; }
        public Vector2 Target => Vector2.Zero;
    }
}
