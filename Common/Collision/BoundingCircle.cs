using System.Numerics;

namespace Common.Collision
{
    public class BoundingCircle : ICollider
    {
        public int Radius { get; set; }
        public Vector2 Position { get; set; }

        public bool Contains(ICollider collider)
        {
            if (collider is BoundingCircle circle)
                return CollisionEx.Contains(this, circle);

            if (collider is BoundingBox box)
                return CollisionEx.Contains(this, box);

            return false;
        }

        public bool Contains(Vector2 point)
        {
            return CollisionEx.Contains(this, point);
        }
    }
}
