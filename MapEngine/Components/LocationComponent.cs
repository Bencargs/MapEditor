using Common;
using Common.Entities;

namespace MapEngine.Components
{
    public class LocationComponent : IComponent
    {
        public ComponentType Type => ComponentType.Location;
        public Point Location { get; set; }
    }
}
