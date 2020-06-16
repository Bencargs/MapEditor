using Common.Entities;
using System.Numerics;

namespace MapEngine.Components
{
    public class MovementComponent : IComponent
    {
        public ComponentType Type => ComponentType.Movement;
        
        public float FacingAngle { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Steering { get; set; }

        public float MaxVelocity { get; set; }
        public float Mass { get; set; }
        public float MaxForce { get; set; }
    }
}
