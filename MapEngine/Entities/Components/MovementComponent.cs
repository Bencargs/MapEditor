using Common;
using Common.Entities;
using MapEngine.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MapEngine.Entities.Components
{
    public class MovementComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Movement;
        
        public Vector3 Velocity { get; set; }
        public Vector2 Steering { get; set; }
        public float BrakeForce { get; set; }

        public float MaxVelocity { get; set; }
        public float MaxGradient { get; set; }
        public float Mass { get; set; }
        public float MaxForce { get; set; }
        public float StopRadius { get; set; }

        public Queue<MoveOrder> Destinations = new Queue<MoveOrder>();
        public TerrainType[] Terrains { get; set; }

        public IComponent Clone()
        {
            return new MovementComponent
            {
                Velocity = Velocity,
                Steering = Steering,
                MaxVelocity = MaxVelocity,
                MaxGradient = MaxGradient,
                Mass = Mass,
                MaxForce = MaxForce,
                StopRadius = StopRadius,
                BrakeForce = BrakeForce,
                Terrains = Terrains,
                Destinations = new Queue<MoveOrder>(Destinations.Select(x => x.Clone()))
            };
        }
    }
}
