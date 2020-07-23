using Common.Entities;
using MapEngine.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MapEngine.Entities.Components
{
    public class MovementComponent : IComponent
    {
        public ComponentType Type => ComponentType.Movement;
        
        public Vector2 Velocity { get; set; }
        public Vector2 Steering { get; set; }
        public float BrakeForce { get; set; }

        public float MaxVelocity { get; set; }
        public float Mass { get; set; }
        public float MaxForce { get; set; }

        public float StopRadius { get; set; }
        public Queue<MoveOrder> Destinations = new Queue<MoveOrder>();

        public IComponent Clone()
        {
            return new MovementComponent
            {
                Velocity = Velocity,
                Steering = Steering,
                MaxVelocity = MaxVelocity,
                Mass = Mass,
                MaxForce = MaxForce,
                StopRadius = StopRadius,
                BrakeForce = BrakeForce,
                Destinations = new Queue<MoveOrder>(Destinations.Select(x => x.Clone()))
            };
        }
    }
}
