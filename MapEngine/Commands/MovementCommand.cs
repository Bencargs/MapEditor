using Common.Entities;
using System.Numerics;

namespace MapEngine.Commands
{
    public class MovementCommand
    {
        public Entity Entity { get; set; }
        public MovementMode MovementMode { get; set; }
        public Vector2 Destination { get; set; }
    }
}
