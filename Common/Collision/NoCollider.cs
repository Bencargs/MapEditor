using System.Numerics;

namespace Common.Collision
{
    public class NoCollider : ICollider
    {
        public Vector2 Location { get; set; }
        public bool HasCollided(ICollider collider) => false;

        public ICollider Clone()
        {
            return this;
        }
    }
}