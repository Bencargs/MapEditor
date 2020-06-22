using System.Numerics;

namespace Common.Collision
{
    public class BoundingBox : ICollider
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 Location { get; set; }

        public bool HasCollided(ICollider collider)
        {
            if (collider is BoundingCircle circle)
                return CollisionEx.Contains(this, circle);

            if (collider is BoundingBox box)
                return CollisionEx.Contains(this, box);

            return false;
        }

        public ICollider Clone()
        {
            return new BoundingBox
            {
                Width = Width,
                Height = Height,
                Location = new Vector2(Location.X, Location.Y)
            };
        }
    }
}
