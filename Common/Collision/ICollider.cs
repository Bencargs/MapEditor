using System.Numerics;

namespace Common.Collision
{
    public interface ICollider
    {
        Vector2 Location { get; set; }

        bool HasCollided(ICollider collider);

        ICollider Clone();
    }
}
