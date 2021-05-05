using System.Collections.Generic;
using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class SensorComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Sensor;
        
        public float Radius { get; set; }
        public List<Entity> Detections { get; set; } = new List<Entity>();
        
        public IComponent Clone()
        {
            return new SensorComponent
            {
                Radius = Radius
            };
        }
    }
}
