using System.Numerics;

namespace Common
{
    public class Camera
    {
        public Rectangle Viewport { get; set; }
        public Rectangle InnerViewport { get; set; }
        public Vector3 Location { get; set; }
        public Vector3 Target => new Vector3(Viewport.X, Viewport.Y, 0);
    }
}
