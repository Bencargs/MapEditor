using Common.Entities;
using MapEngine.Handlers;
using System.Numerics;

namespace MapEngine.Commands
{
    public class MoveCommand : ICommand
    {
        public Entity Entity { get; set; }
        public bool Queue { get; set; }
        public Vector2 Destination { get; set; }
        public MovementMode MovementMode { get; set; }
    }
}
