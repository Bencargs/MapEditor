using MapEditor.Entities;
using MapEditor.Handlers.CollisionHandler;

namespace MapEditor.Components
{
    public class CollisionComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Collision;
        public ICollider Collider { get; set; }
    }
}
