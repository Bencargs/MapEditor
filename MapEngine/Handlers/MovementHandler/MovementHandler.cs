﻿using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MapEngine.Entities;
using MapEngine.Services.PathfindingService;
using MapEngine.Services.Map;

namespace MapEngine.Handlers
{
    public class MovementHandler 
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
        , IHandleCommand<MoveCommand>
    {
        private readonly MapService _mapService;
        private readonly PathfindingService _pathfinding;
        private readonly List<Entity> _entities = new List<Entity>();

        public MovementHandler(
            MapService mapService,
            PathfindingService pathfindingService)
        {
            _mapService = mapService;
            _pathfinding = pathfindingService;
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

            ApplyGravity(location, movement);

            // pathing into un-navigable terrain - hard stop
            var tile = _mapService.GetTile(location.Location);
            if (!entity.IsNavigable(tile))
            {
                movement.Velocity = Vector3.Zero;
            }

            if (TryGetTarget(location, movement, out var target))
            {
                switch (target.MovementMode)
                {
                    case MovementMode.Direct:
                        location.Location += movement.Velocity.ToVector2();
                        break;
                    case MovementMode.Seek:
                        //todo: would a better way be:
                        // 1. Update rotation
                        // 2. Update Speed
                        // 3. Update Position?
                        Seek(location, movement, target.Destination);
                        ApplyFriction(location, movement);
                        break;
                }
            }
            else if (movement.Velocity.Length() > 0)
            {
                // eg. an entity having a force applied to it
                location.Location += movement.Velocity.ToVector2();
                ApplyFriction(location, movement);
            }
        }

        private void ApplyGravity(LocationComponent location, MovementComponent movement)
        {
            //todo: this is ick
            const int Gravity = -1;// 9.81 in tenths of meters rounded to int

            var mapHeight = _mapService.GetElevation(location.Location);

            // Eg. airborne - apply gravity to velocity, and apply velocity to height
            movement.Velocity += new Vector3(0, 0, Gravity);
            location.Elevation += (int)movement.Velocity.Z;

            // Eg. on surface - follow terrain and remove downward velocity
            if (location.Elevation <= mapHeight)
            {
                movement.Velocity = new Vector3(movement.Velocity.X, movement.Velocity.Y, 0);
                location.Elevation = mapHeight;
            }
        }


        private void ApplyFriction(LocationComponent location, MovementComponent movement)
        {
            var friction = _mapService.GetFriction(location.Location);

            movement.Velocity *= friction;
        }

        private static void ApplyBrakeForce(MovementComponent movement)
        {
            movement.Velocity -= movement.Velocity.Truncate(movement.BrakeForce);
        }

        private bool TryGetTarget(LocationComponent location, MovementComponent movement, out MoveOrder target)
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
            return Math.Abs(Vector2.Distance(location, target)) < stopRadius;
        }

        private static void Seek(LocationComponent location, MovementComponent target, Vector2 destination)
        {
            //subtract the position from the target to get the vector from the vehicles position to the target. 
            var directionVector = (destination - location.Location);

            //Normalize it then multiply by max speed to get the maximum velocity from your position to the target.
            var desiredVelocity = directionVector.Truncate(target.MaxVelocity);

            //subtract velocity from the desired velocity to get the force vector
            target.Steering = desiredVelocity - target.Velocity.ToVector2();

            //divide the steeringForce by the mass(which makes it the acceleration), 
            target.Steering = target.Steering.Truncate(target.MaxForce) / target.Mass;

            //then add it to velocity to get the new velocity
            var targetVelocity2 = (target.Velocity.ToVector2() + target.Steering).Truncate(target.MaxVelocity);
            target.Velocity = new Vector3(targetVelocity2.X, targetVelocity2.Y, 0);

            //Set the facingAngle (used to draw the image, in radians) to velocity
            location.FacingAngle = target.Velocity.ToVector2().Angle();

            location.Location += target.Velocity.ToVector2();
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
            var entityDestinations = FormationService.GetFormationPositions(command.Entities, command.Destination);
            foreach (var entity in command.Entities)
            {
                var movementComponent = entity.GetComponent<MovementComponent>();

                var destination = entityDestinations[entity];
                var path = _pathfinding.GetPath(entity, destination);
                var orders = ToMoveOrders(path, command.MovementMode);

                if (!command.Queue)
                    movementComponent.Destinations.Clear();

                movementComponent.Destinations.Enqueue(orders);
            }
        }

        private static IEnumerable<MoveOrder> ToMoveOrders(List<Tile> path, MovementMode movementMode) =>
            path.Select(x => new MoveOrder
            {
                MovementMode = movementMode,
                Destination = new Vector2(x.Location.X, x.Location.Y)
            });
    }
}
