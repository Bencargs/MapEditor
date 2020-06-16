using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Components;
using System.Numerics;

namespace MapEngine.Handlers
{
    public class MovementHandler
    {
        public float Friction = 0.98f;//Map friction

        public void Handle(MovementCommand command)
        {
            var entity = command.Entity;
            var location = entity.GetComponent<LocationComponent>();
            var target = entity.GetComponent<MovementComponent>();

            if (!Contains(location.Location, command.Destination))
            {
                Seek(location, target, command);
            }

            target.Velocity *= Friction;
            location.Location = location.Location + target.Velocity;
        }

        // replace with extensions or something - adding a region around each point for collision purposes
        private bool Contains(Vector2 a, Vector2 b)
        {
            var radius = 10;
            var areaA = new Rectangle((int)a.X - radius, (int)a.Y - radius, radius * 2, radius * 2);
            var areaB = new Rectangle((int)b.X - radius, (int)b.Y - radius, radius * 2, radius * 2);

            return areaA.X <= areaB.X + areaB.Width &&
                   areaB.X <= areaA.X + areaA.Width &&
                   areaA.Y <= areaB.Y + areaB.Height &&
                   areaB.Y <= areaA.Y + areaA.Height;
        }

        private void Seek(LocationComponent location, MovementComponent target, MovementCommand command)
        {
            //subtract the position from the target to get the vector from the vehicles position to the target. 
            var directionVector = (command.Destination - location.Location)/* / Data.MaxVelocity*/;

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
