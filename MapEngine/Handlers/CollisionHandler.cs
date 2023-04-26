using Common.Collision;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MapEngine.Entities;
using System;
using MapEngine.Factories;

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
        private readonly MessageHub _messageHub;

        public CollisionHandler(MessageHub messageHub)
        {
            _messageHub = messageHub;
        }

        public void Update()
        {
            foreach (var e in _entities)
            {
                HandleCollisions(e);
            }
        }

        private void HandleCollisions(Entity entity)
        {
            // todo - ?? seems like units are colliding with their own projectiles, and exploding?
            //if (entity.Id != 72)
            //    return;

            var collider = entity.GetComponent<CollisionComponent>();
            if (collider == null)
                return;

            if (HasCollided(entity, out var impactors))
            {
                foreach (var i in impactors)
                {
                    var sourceLocation = entity.GetComponent<LocationComponent>();
                    var sourceVelocity = entity.GetComponent<MovementComponent>();
                    var force = GetImpactForce(sourceVelocity);
                    if (force > 0)
                    {
                        // If its a meaningful impact, modify velocity
                        var selfLocation = i.GetComponent<LocationComponent>();
                        var selfVelocity = i.GetComponent<MovementComponent>();

                        //https://gamedev.stackexchange.com/questions/15911/how-do-i-calculate-the-exit-vectors-of-colliding-projectiles
                        var collisionVector = (selfLocation.Location - sourceLocation.Location).Normalize();
                        var selfForce = Vector2.Dot(selfVelocity.Velocity, collisionVector);
                        var sourceForce = Vector2.Dot(sourceVelocity.Velocity, collisionVector);

                        // todo: involve mass of each object in calculating reflection angle and speed
                        // Math.Min stops objects 'sticking' to each other
                        var optimisedP = (float) Math.Min(0, 2.0 * (selfForce - sourceForce));
                        
                        var newSelfVelocity = selfVelocity.Velocity - optimisedP * collisionVector;

                        selfVelocity.Velocity = newSelfVelocity;
                    }

                    if (force > collider.MaxImpactForce)
                    {
                        var location = entity.GetComponent<LocationComponent>();
                        ParticleFactory.TryGetParticle("Flash1", out var particle1);
                        _messageHub.Post(new CreateEntityCommand
                        {
                            Entity = new Entity
                            {
                                Components = new List<IComponent>
                                {
                                    particle1,
                                    new LocationComponent
                                    {
                                        Location = location.Location
                                    }
                                }
                            }
                        });

                        ParticleFactory.TryGetParticle("Shrapnel1", out var particle3);
                        _messageHub.Post(new CreateEntityCommand
                        {
                            Entity = new Entity
                            {
                                Components = new List<IComponent>
                                {
                                    particle3,
                                    new LocationComponent
                                    {
                                        Location = location.Location
                                    }
                                }
                            }
                        });

                        ParticleFactory.TryGetParticle("Explosion1", out var particle2);
                        _messageHub.Post(new CreateEntityCommand
                        {
                            Entity = new Entity
                            {
                                Components = new List<IComponent>
                                {
                                    particle2,
                                    new LocationComponent
                                    {
                                        Location = location.Location
                                    }
                                }
                            }
                        });
                        _messageHub.Post(new DestroyEntityCommand { Entity = i });
                    }
                }
            }
        }

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

        public float GetImpactForce(MovementComponent movementComponent)
        {
            if (movementComponent == null)
                return 0f;

            var impactForce = 0.5f * movementComponent.Mass * movementComponent.Velocity.LengthSquared();
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
