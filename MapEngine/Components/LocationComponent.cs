using Common.Entities;
using System.Numerics;

namespace MapEngine.Components
{
    public class LocationComponent : IComponent
    {
        public ComponentType Type => ComponentType.Location;
        public Vector2 Location { get; set; }
    }
}
