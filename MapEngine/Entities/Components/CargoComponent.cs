using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class CargoComponent : IComponent
    {
        public ComponentType Type => ComponentType.Cargo;

        public Vector2? Destination = null;

        public int Capacity { get; set; }
        public List<Entity> Content { get; set; } = new List<Entity>();
        public Vector2? UnloadPoint { get; set; }
        public float StopRadius { get; set; }
        public float UnloadVelocity { get; set; }

        public IComponent Clone()
        {
            return new CargoComponent
            {
                Content = Content.ToList(),
                Destination = Destination,
                Capacity = Capacity,
                UnloadPoint = UnloadPoint,
                StopRadius = StopRadius,
                UnloadVelocity = UnloadVelocity
            };
        }
    }
}