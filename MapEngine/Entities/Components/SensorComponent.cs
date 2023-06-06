using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common.Collision;
using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class SensorComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Sensor;
        
        public string Name { get; set; }
        public float Radius { get; set; }
        public BoundingPolygon VisibilityRaycast { get; set; }
        public List<Entity> Detections { get; set; } = new List<Entity>();
        
        public IComponent Clone()
        {
            return new SensorComponent
            {
                Name = Name,
                Radius = Radius,
                VisibilityRaycast = (BoundingPolygon) VisibilityRaycast?.Clone(),
                Detections = Detections.ToList()
            };
        }
    }
}
