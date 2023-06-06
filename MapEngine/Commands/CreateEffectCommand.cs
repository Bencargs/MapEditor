using System.Numerics;

namespace MapEngine.Commands
{
    public class CreateEffectCommand : ICommand
    {
        public string Name { get; set; }
        public Vector2 Location { get; set; }
        public float Value { get; set; }
    }
}
