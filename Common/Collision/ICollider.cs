using System.Numerics;

namespace Common.Collision
{
    public interface ICollider
    {
        Vector2 Position { get; set; }

        bool Contains(ICollider collider);
        bool Contains(Vector2 point);
    }
}
