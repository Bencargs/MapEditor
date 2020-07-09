using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MapEngine.Handlers
{
    public class MovementHandler 
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly MessageHub _messageHub;
        private readonly CollisionHandler _collisionHandler;
        private readonly List<Entity> _entities = new List<Entity>();

        public MovementHandler(MessageHub messageHub, CollisionHandler collisionHandler)
        {
            _messageHub = messageHub;
            _collisionHandler = collisionHandler;
        }

        public void Update()
        {
            foreach (var e in _entities)
            {
                HandleMovement(e);
            }
        }

        public void HandleMovement(Entity entity)
        {
            var location = entity.GetComponent<LocationComponent>();
            var movement = entity.GetComponent<MovementComponent>();
            if (location == null || movement == null)
                return;

            if (TryGetTarget(location, movement, out var target))
            {
                switch (target.MovementMode)
                {
                    case MovementMode.Seek:
                        Seek(location, movement, target.Destination);
                        break;
                }
            }
            ApplyFriction(location, movement);
            HandleCollisions(entity);
        }

        private void HandleCollisions(Entity entity)
        {
            var collider = entity.GetComponent<CollisionComponent>();
            if (collider == null)
                return;

            if (_collisionHandler.HasCollided(entity, out var _))
            {
                var impactForce = _collisionHandler.GetImpactForce(entity);
                if (impactForce > collider.MaxImpactForce)
                {
                    // todo: explosions!
                    //var explosion = ExplosionFactory.Create(entity);
                    //_messageHub.Post(new CreateEntityCommand{ Entity = explosion });
                    _messageHub.Post(new DestroyEntityCommand { Entity = entity });
                }
            }
        }

        private static void ApplyFriction(LocationComponent location, MovementComponent movement)
        {
            const float Friction = 0.95f; // Ideally this would be a property of the map tile

            movement.Velocity *= Friction;
            location.Location += movement.Velocity;
        }

        private static void ApplyBrakeForce(MovementComponent movement)
        {
            movement.Velocity -= movement.Velocity.Truncate(movement.BrakeForce);
        }

        private static bool TryGetTarget(LocationComponent location, MovementComponent movement, out MoveOrder target)
        {
            target = movement.Destinations.FirstOrDefault();
            if (target == null)
                return false;

            if (HasArrived(location.Location, target.Destination, movement.StopRadius))
            {
                movement.Destinations.Dequeue();
                ApplyBrakeForce(movement);
            }

            return true;
        }

        private static bool HasArrived(Vector2 location, Vector2 target, float stopRadius)
        {
            return Math.Abs(location.Distance(target)) < stopRadius;
        }

        private static void Seek(LocationComponent location, MovementComponent target, Vector2 destination)
        {
            //subtract the position from the target to get the vector from the vehicles position to the target. 
            var directionVector = (destination - location.Location);

            //Set the facingAngle (used to draw the image, in radians) to velocity
            target.FacingAngle = target.Velocity.Angle();

            //Normalize it then multiply by max speed to get the maximum velocity from your position to the target.
            var desiredVelocity = directionVector.Truncate(target.MaxVelocity);

            //subtract velocity from the desired velocity to get the force vector
            target.Steering = desiredVelocity - target.Velocity;

            //divide the steeringForce by the mass(which makes it the acceleration), 
            target.Steering = (target.Steering.Truncate(target.MaxForce)) / target.Mass;

            //then add it to velocity to get the new velocity
            target.Velocity = (target.Velocity + target.Steering).Truncate(target.MaxVelocity);

            location.Location += target.Velocity;
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (entity.GetComponent<MovementComponent>() == null)
                return;

            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
