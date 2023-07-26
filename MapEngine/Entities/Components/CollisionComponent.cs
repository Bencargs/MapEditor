using System.Collections.Generic;
using System.Linq;
using Common.Collision;
using Common.Entities;
using System.Numerics;
using System.Windows.Documents;

namespace MapEngine.Entities.Components
{
    public class CollisionComponent : IComponent
    {
        public ComponentType Type => ComponentType.Collision;
        public float MaxImpactForce { get; set; }
        public List<Entity> Ignore { get; set; } = new List<Entity>();

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
            return new CollisionComponent(_collider)
            {
                MaxImpactForce = MaxImpactForce,
                Ignore = Ignore.ToList()
            };
        }
    }
}
