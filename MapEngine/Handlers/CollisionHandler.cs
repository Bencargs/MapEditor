using Common.Collision;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MapEngine.Entities;

namespace MapEngine.Handlers
{
    /// <summary>
    /// Responsible for determining if collisions occur for collider entities
    /// </summary>
    public class CollisionHandler 
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly List<Entity> _entities = new List<Entity>();

        public bool HasCollided(Entity entity, out List<Entity> collisions)
        {
            collisions = new List<Entity>();
            var collider = entity.Hitbox();
            if (collider == null)
                return false;
            
            collisions = GetCollisions(collider)
                .Where(x => x.entity.Id != entity.Id)
                .OrderBy(x => x.distance)
                .Select(x => x.entity).ToList();

            return collisions.Any();
        }

        public double GetImpactForce(Entity source)
        {
            var movementComponent = source.GetComponent<MovementComponent>();
            if (movementComponent == null)
                return 0d;

            var impactForce = 0.5 * movementComponent.Mass * movementComponent.Velocity.LengthSquared();
            return impactForce;
        }

        public IEnumerable<(Entity entity, float distance)> GetCollisions(ICollider source)
        {
            foreach (var e in _entities)
            {
                var targetLocation = e.Location();
                var target = e.GetComponent<CollisionComponent>().GetCollider(targetLocation);
                if (source.HasCollided(target))
                {
                    var distance = Vector2.Distance(source.Location, targetLocation);
                    yield return (e, distance);
                }
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var collider = command.Entity.GetComponent<CollisionComponent>();
            if (collider == null)
                return;

            _entities.Add(command.Entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
