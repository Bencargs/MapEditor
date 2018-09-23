using MapEditor.Components;

namespace MapEditor.Commands
{
    public class UnitComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Unit;
        public string Title { get; set; }
        public string Name { get; set; }
    }
}
