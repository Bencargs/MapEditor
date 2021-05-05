using Common.Collision;
using Common.Entities;
using System.Numerics;

namespace MapEngine.Entities.Components
{
    public class CollisionComponent : IComponent
    {
        public ComponentType Type => ComponentType.Collision;
        public float MaxImpactForce { get; set; }

        private readonly ICollider _collider;
        public ICollider GetCollider(Vector2 location)
        {
            // todo - whats going on here?
            var c = _collider.Clone();
            c.Location = location;
            return c;
        }

        public CollisionComponent(ICollider collider)
        {
            _collider = collider;
        }

        public IComponent Clone()
        {
            return new CollisionComponent(_collider);
        }
    }
}
