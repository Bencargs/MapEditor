using System.Drawing;
using MapEditor.Common;

namespace MapEditor.Handlers.CollisionHandler
{
    public class BoundingCircle : ICollider
    {
        public ColliderType Type { get; } = ColliderType.BoundingCircle;
        public float Radius { get; set; }
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
