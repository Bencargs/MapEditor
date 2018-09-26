using System;
using System.Collections.Generic;
using System.Linq;
using MapEditor.Common;
using MapEditor.Components;
using MapEditor.Controllers.MovementHandler;
using MapEditor.Engine;
using MapEditor.Entities;

namespace MapEditor.Handlers.MovementHandler
{
    public class MovementHandler : IComponentHandler
    {
        // todo: ensure this is checked on loan, not during game runtime
        public IEnumerable<Type> RequiredComponents { get; } = new[]
        {
            typeof(PositionComponent),
            typeof(MovementComponent),
            typeof(PhysicsComponent),
            typeof(PathingComponent)
        };

        private readonly CollisionHandler.CollisionHandler _collisions;

        public MovementHandler(CollisionHandler.CollisionHandler collisionHandler)
        {
            _collisions = collisionHandler;
        }

        public void Handle(Entity entity, double elapsed)
        {
            //todo: incorporate elapsed some how

            // todo: make a GetComponents() method
            var positionData = entity.GetComponent<PositionComponent>();
            var movementData = entity.GetComponent<MovementComponent>();
            var physicsData = entity.GetComponent<PhysicsComponent>();
            var pathingData = entity.GetComponent<PathingComponent>();

            var destinations = pathingData.Destinations;
            var position = positionData.Position;
            var velocity = movementData.Velocity;
            var slowRadius = movementData.SlowRadius;
            var stopRadius = movementData.StopRadius;
            var maxVelocity = movementData.MaxVelocity;
            var force = movementData.Force;
            var mass = physicsData.Mass;

            ITarget target;
            Vector2 steering = null;    //todo: save previous steering, then do steering += Move()
            switch (movementData.MovementMode)
            {
                case MovementMode.Move:
                    target = DequeueFirstTarget(destinations, position, stopRadius);
                    if (target != null)
                        steering = Move(target.Position,
                            position,
                            velocity,
                            slowRadius,
                            maxVelocity,
                            force,
                            mass);
                    break;
                case MovementMode.Intercept:
                    target = DequeueFirstTarget(destinations, position, stopRadius);
                    if (target != null)
                        steering = Intercept(target.Position,
                            target.Velocity,
                            position,
                            velocity,
                            slowRadius,
                            maxVelocity,
                            force,
                            mass);
                    break;
                case MovementMode.Follow:
                    target = GetFirstTarget(destinations, position, stopRadius);
                    break;
                case MovementMode.Evade:
                    target = GetFirstTarget(destinations, position, stopRadius);
                    break;
                case MovementMode.Roam:
                    target = GetRandomTarget(destinations, position, stopRadius);
                    break;
                case MovementMode.Patrol:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // todo: If steering is null, set some kind of stationary flag

            steering += CollisionAvoidance(position, velocity, steering, maxVelocity);

            positionData.Position = Update(steering, velocity, position,
                force, mass, maxVelocity);
        }

        private static Vector2 Update(Vector2 steering,
            Vector2 velocity,
            Vector2 position,
            float force,
            float mass,
            float maxVelocity)
        {
            //(velocity + steering).Truncate(maxVelocity)
            //position += velocity

            steering = steering.Truncate(force);

            steering = steering * (1 / mass);

            velocity += steering;
            velocity.Truncate(maxVelocity);

            return position + velocity;
        }

        private Vector2 CollisionAvoidance(Vector2 position,
                                           Vector2 velocity,
                                           Vector2 steering,
                                           float maxVelocity)
        {
            var dynamicLength = velocity.Length() / maxVelocity;
            var lookAhead = (position + velocity.Normalize()) * dynamicLength;
            var obstacle = _collisions.GetCollision(lookAhead);
            if (obstacle != null)
            {
                var obstaclePosition = new Vector2 {X = obstacle.Position.X, Y = obstacle.Position.Y};
                var force = obstaclePosition - position;
                steering += Evade(obstaclePosition, force);
            }

            return steering;
        }

        private static ITarget DequeueFirstTarget(IList<ITarget> destinations,
            Vector2 position,
            float stopRadius)
        {
            var target = destinations.FirstOrDefault();
            if (HasArrived(position, target, stopRadius))
            {
                destinations.RemoveAt(0);
            }
            return target;
        }

        private static ITarget EnqueueFirstTarget(IList<ITarget> destinations,
            Vector2 position,
            float stopRadius)
        {
            var target = destinations.FirstOrDefault();
            if (HasArrived(position, target, stopRadius))
            {
                destinations.RemoveAt(0);
                destinations.Add(target);
            }
            return target;
        }

        private static ITarget GetFirstTarget(IList<ITarget> destinations,
            Vector2 position,
            float stopRadius)
        {
            var target = destinations.FirstOrDefault();
            if (HasArrived(position, target, stopRadius))
            {
                destinations.RemoveAt(0);
                destinations.Add(target);
            }
            return target;
        }

        private static ITarget GetRandomTarget(IList<ITarget> destinations,
            Vector2 position,
            float stopRadius)
        {
            var target = destinations.FirstOrDefault();
            if (HasArrived(position, target, stopRadius))
            {
                //Generate a random target in a small arear
                destinations.RemoveAt(0);
                //destinations.Insert(0, )
            }
            return target;
        }

        private static bool HasArrived(Vector2 position, ITarget target, float stopRadius)
        {
            return target != null && position.Distance(target.Position) >= stopRadius;
        }

        private static Vector2 Move(Vector2 targetPosition,
            Vector2 position,
            Vector2 velocity,
            float slowRadius,
            float maxVelocity,
            float force,
            float mass)
        {
            //subtract the position from the target to get the vector from the vehicles position to the target. 
            var distance = targetPosition - position/* / Data.MaxVelocity*/;

            //Normalize it then multiply by max speed to get the maximum velocity from your position to the target.
            var desiredVelocity = distance.Truncate(maxVelocity); // distance.Normalize()

            //if (distance.Length() <= slowRadius)
            //desiredVelocity =* (maxVelocity * distance / slowRadius);
            //else
            //    desiredVelocity.ScaleBy(movement.MaxVelocity);

            //subtract velocity from the desired velocity to get the force vector
            var steering = desiredVelocity - velocity;

            //divide the steeringForce by the mass(which makes it the acceleration), 
            steering = steering.Truncate(force) / mass;

            return steering;
        }

        private static Vector2 Intercept(Vector2 targetPosition,
            Vector2 targetVelocity,
            Vector2 position,
            Vector2 velocity,
            float slowRadius,
            float maxVelocity,
            float force,
            float mass)
        {
            var distance = targetPosition - position;

            var updatesNeeded = distance / maxVelocity;

            var targetVector = targetVelocity * updatesNeeded;

            return Move(targetVector,
                position,
                velocity,
                slowRadius,
                maxVelocity,
                force,
                mass);
        }

        private void Follow(ITarget target)
        {

        }

        private Vector2 Evade(Vector2 targetPosition, Vector2 targetForce)
        {
            return null;
        }

        private void Roam(ITarget target)
        {

        }
    }
}
