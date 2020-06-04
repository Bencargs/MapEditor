using MapEditor.Common;

namespace MapEditor.Components
{
    public class PositionComponent : IComponent
    {
        public ComponentType Type { get; set; } = ComponentType.Position;
        public Vector2 Position { get; set; }

        // todo: where neccessary:
        //Set the facingAngle (used to draw the image, in radians) to velocity
        //FacingAngle = Data.Velocity.Angle();
    }
}
