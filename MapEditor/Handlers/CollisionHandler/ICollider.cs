using System.Drawing;

namespace MapEditor.Controllers.CollisionHandler
{
    public interface ICollider
    {
        Point Position { get; set; }
        bool IsCollided(ICollider collider);
    }
}
