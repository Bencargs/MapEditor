using Common.Collision;
using Common.Entities;
using System.Numerics;

namespace MapEngine.Entities.Components
{
    public class CollisionComponent : IComponent
    {
        public ComponentType Type => ComponentType.Collision;
        public float MaxImpactForce { get; set; }
        
        private ICollider _collider { get; set; }
        public ICollider GetCollider(Vector2 location)
        {
            _collider.Location = location;
            return _collider;
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
