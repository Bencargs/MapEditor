using System.Collections.Generic;
using System.Numerics;
using Common.Entities;

namespace MapEngine.Commands
{
    public class UnloadCommand : ICommand
    {
        public List<Entity> Entities { get; set; } = new List<Entity>();

        public Vector2 Destination = Vector2.Zero;
    }
}