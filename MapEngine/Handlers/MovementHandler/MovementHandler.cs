﻿using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MapEngine.Handlers
{
    public class MovementHandler 
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
        , IHandleCommand<MoveCommand>
    {
        private readonly NavigationService _navigator;
        private readonly MessageHub _messageHub;
        private readonly CollisionHandler _collisionHandler;
        private readonly List<Entity> _entities = new List<Entity>();

        public MovementHandler(
            MessageHub messageHub, 
            CollisionHandler collisionHandler,
            NavigationService navigationService)
        {
            _messageHub = messageHub;
            _navigator = navigationService;
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
                    case MovementMode.Direct:
                        location.Location += movement.Velocity;
                        break;
                    case MovementMode.Seek:
                        Seek(location, movement, target.Destination);
                        ApplyFriction(movement);
                        break;
                }
            }
            HandleCollisions(entity);
        }

        private void HandleCollisions(Entity entity)
        {
            // todo - ?? seems like units are colliding with their own projectiles, and exploding?
            //if (entity.Id != 72)
            //    return;

            var collider = entity.GetComponent<CollisionComponent>();
            if (collider == null)
                return;

            if (_collisionHandler.HasCollided(entity, out var _))
            {
                var impactForce = _collisionHandler.GetImpactForce(entity);
                if (impactForce > collider.MaxImpactForce)
                {
                    // todo: explosions!
                    //var explosion = ParticleFactory.Create(entity);
                    //_messageHub.Post(new CreateEffectCommand { Entity = explosion });
                    _messageHub.Post(new DestroyEntityCommand { Entity = entity });
                }
            }
        }

        private static void ApplyFriction(MovementComponent movement)
        {
            const float Friction = 0.95f; // Ideally this would be a property of the map tile

            movement.Velocity *= Friction;
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

            //Normalize it then multiply by max speed to get the maximum velocity from your position to the target.
            var desiredVelocity = directionVector.Truncate(target.MaxVelocity);

            //subtract velocity from the desired velocity to get the force vector
            target.Steering = desiredVelocity - target.Velocity;

            //divide the steeringForce by the mass(which makes it the acceleration), 
            target.Steering = (target.Steering.Truncate(target.MaxForce)) / target.Mass;

            //then add it to velocity to get the new velocity
            target.Velocity = (target.Velocity + target.Steering).Truncate(target.MaxVelocity);

            //Set the facingAngle (used to draw the image, in radians) to velocity
            location.FacingAngle = target.Velocity.Angle();

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

        public void Handle(MoveCommand command)
        {
            var movementComponent = command.Entity.GetComponent<MovementComponent>();

            var path = _navigator.GetPath(command.Entity, command.Destination);
            var orders = ToMoveOrders(path, command.MovementMode);

            if (!command.Queue)
                movementComponent.Destinations.Clear();

            movementComponent.Destinations.Enqueue(orders);
        }

        private IEnumerable<MoveOrder> ToMoveOrders(Tile[] path, MovementMode movementMode) =>
            path.Select(x =>
            {
                var centerX = x.Size / 2;
                var centerY = x.Size / 2;
                return new MoveOrder
                {
                    MovementMode = movementMode,
                    Destination = new Vector2(x.Location.X + centerX, x.Location.Y + centerY)
                };
            });
    }
}
