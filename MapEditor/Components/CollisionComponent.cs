using MapEditor.Controllers.CollisionHandler;
using MapEditor.Entities;

namespace MapEditor.Components
{
    public class CollisionComponent : IComponent
    {
        public ICollider Collider { get; set; }
    }
}
