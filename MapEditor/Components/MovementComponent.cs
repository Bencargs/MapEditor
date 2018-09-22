using System.Collections.Generic;
using MapEditor.Common;
using MapEditor.Controllers.MovementHandler;
using MapEditor.Engine;

namespace MapEditor.Components
{
    public class MovementComponent : IComponent
    {
        // todo: rather than a single mode, it is possible to have multiple modes
        // eg- seek this target, while avoiding this area - replace with flag instead?
        public MovementMode MovementMode { get; set; }
        public List<TerrainType> TerrainTypes { get; set; }

        public Vector2 Velocity { get; set; }
        public float MaxVelocity { get; set; }
        public float Force { get; set; }
        public float SlowRadius { get; set; }
        public float StopRadius { get; set; }
    }
}
