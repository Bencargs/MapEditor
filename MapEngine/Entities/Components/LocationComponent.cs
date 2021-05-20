using System.Numerics;
using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class LocationComponent : IComponent
    {
        public ComponentType Type => ComponentType.Location;
        public Vector2 Location { get; set; }
        public float FacingAngle { get; set; }

        public IComponent Clone()
        {
            return new LocationComponent
            {
                Location = Location,
                FacingAngle = FacingAngle
            };
        }
    }
}
