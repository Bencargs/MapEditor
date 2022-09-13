using Common.Collision;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MapEngine.Entities;

namespace MapEngine.Handlers
{
    /// <summary>
    /// Responsible for weapon aiming, firing, and applying damage
    /// </summary>
    public class WeaponHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly MessageHub _messageHub;
        private readonly CollisionHandler _collisionHandler;
        private readonly List<Entity> _entities = new List<Entity>();

        public WeaponHandler(MessageHub messageHub, CollisionHandler collisionHandler)
        {
            _messageHub = messageHub;
            _collisionHandler = collisionHandler;
        }

        public void Update()
        {
            foreach (var e in _entities)
            {
                var weaponComponent = e.GetComponent<WeaponComponent>();
                var now = DateTime.Now;// todo: weaponComponent.GetElapsedTime()
                var elapsed = (now - weaponComponent.LastFiredTime).TotalMilliseconds;
                if (elapsed < weaponComponent.ReloadTime)
                    continue;

                var location = e.GetComponent<LocationComponent>(); // todo: replace these with entity extension methods?
                var collider = new BoundingCircle { Radius = weaponComponent.Range, Location = location.Location };
                var collisions = _collisionHandler.GetCollisions(collider).Where(x => x.entity.Id != e.Id);
                if (!collisions.Any())
                    continue;

                // get closest target in range -- in future we could have an AI component with target priorities here
                var target = collisions.First().entity;
                if (target.GetComponent<UnitComponent>().TeamId == e.GetComponent<UnitComponent>().TeamId) // todo: some kind of alliance lookup?
                    continue;

                if (!TryGetAim(e, target, out var aimPoint))
                    continue;

                var projectile = CreateProjectile(location, aimPoint, weaponComponent);
                _messageHub.Post(new CreateEntityCommand { Entity = projectile });

                weaponComponent.LastFiredTime = now;
            }
        }

        private Entity CreateProjectile(LocationComponent location, Vector2 target, WeaponComponent weaponComponent)
        {
            var projectile = new Entity
            {
                Id = 72, // todo: EntityFactory
                Components = new List<IComponent>
                {
                    location.Clone(),
                    new CollisionComponent (new BoundingCircle { Radius = weaponComponent.CollisionRadius } ), // should this be definable?
                    new ImageComponent { TextureId = weaponComponent.TextureId },
                    new MovementComponent
                    {
                        Velocity = target,
                        Steering = Vector2.Zero,
                        MaxVelocity = weaponComponent.Speed,
                        Mass = 1,
                        MaxForce = weaponComponent.MaxImpactForce,
                        Destinations = new Queue<MoveOrder>(new[]
                        {
                            new MoveOrder
                            {
                                MovementMode = MovementMode.Direct,
                                Destination = target
                            }
                        })
                    },
                }
            };
            return projectile;
        }

        private static bool TryGetAim(Entity self, Entity target, out Vector2 aimPoint)
        {
            aimPoint = Vector2.Zero;
            var weapon = self.GetComponent<WeaponComponent>();
            var selfLocation = self.Location();
            var targetLocation = target.Location();
            var targetVelocity = target.GetComponent<MovementComponent>()?.Velocity ?? Vector2.Zero;

            // calculate target and projectiles location at each step in the future
            // return the leading aim location on the first possible intercept
            var maxTime = weapon.Range / weapon.Speed;
            for (var t = 0; t < maxTime; t++)
            {
                targetLocation += targetVelocity;
                var distance = Vector2.Distance(selfLocation, targetLocation);
                if (distance > weapon.Range)
                    continue; // target outside of maximum weapon range

                var traveledDistance = weapon.Speed * t;
                if (traveledDistance < distance)
                    continue; // projectile cannot reach target in time

                aimPoint = (targetLocation - selfLocation).Truncate(weapon.Speed);
                return true;
            }
            return false;
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (entity.GetComponent<WeaponComponent>() == null)
                return;

            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
