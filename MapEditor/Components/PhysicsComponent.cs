namespace MapEditor.Components
{
    public class PhysicsComponent : IComponent
    {
        public ComponentType Type { get; set; } = ComponentType.Physics;
        public float Mass { get; set; }
    }
}
