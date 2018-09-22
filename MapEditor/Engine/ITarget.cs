using MapEditor.Common;

namespace MapEditor.Engine
{
    // todo: rename this
    // any object that can be a movement destination must implement this interface
    // eg: point on make is X & Y all other propertys are null
    // follow a unit - get these properties from the unit
    public interface ITarget
    {
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        float MaxVelocity { get; set; }
        float Mass { get; set; }   //todo: should this be a seperate component?
    }
}
