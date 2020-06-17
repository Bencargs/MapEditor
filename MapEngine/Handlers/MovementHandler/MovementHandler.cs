using Common.Entities;
using MapEngine.Components;
using System;
using System.Linq;
using System.Numerics;

namespace MapEngine.Handlers
{
    public class MovementHandler
    {
        public void Handle(Entity entity)
        {
            var location = entity.GetComponent<LocationComponent>();
            var movement = entity.GetComponent<MovementComponent>();

            if (HasGetTarget(location, movement, out var target))
            {
                switch (target.MovementMode)
                {
                    case MovementMode.Seek:
                        Seek(location, movement, target.Destination);
                        break;
                }
            }
            ApplyFriction(location, movement);
        }

        private void ApplyFriction(LocationComponent location, MovementComponent movement)
        {
            const float Friction = 0.95f; // Ideally this would be a property of the map tile

            movement.Velocity *= Friction;
            location.Location += movement.Velocity;
        }

        private static bool HasGetTarget(LocationComponent location, MovementComponent movement, out MoveOrder target)
        {
            target = movement.Destinations.FirstOrDefault();
            if (target == null)
                return false;
            
            if (HasArrived(location.Location, target.Destination, movement.StopRadius))
                movement.Destinations.Dequeue();

            return true;
        }

        private static bool HasArrived(Vector2 location, Vector2 target, float stopRadius)
        {
            return Math.Abs(location.Distance(target)) < stopRadius;
        }

        private void Seek(LocationComponent location, MovementComponent target, Vector2 destination)
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
    }
}
