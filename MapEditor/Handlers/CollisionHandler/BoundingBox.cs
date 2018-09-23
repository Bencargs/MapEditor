using System.Drawing;
using MapEditor.Common;

namespace MapEditor.Handlers.CollisionHandler
{
    public class BoundingBox : ICollider
    {
        //todo: Non-axis alligned: https://yal.cc/rot-rect-vs-circle-intersection/

        public ColliderType Type { get; } = ColliderType.BoundingBox;
        public float Width { get; set; }
        public float Height { get; set; }
        public Point Position { get; set; }

        public bool IsCollided(ICollider collider)
        {
            if (collider is BoundingCircle circle)
                return this.Contains(circle);

            if (collider is BoundingBox box)
                return this.Contains(box);

            return false;
        }
    }
}
