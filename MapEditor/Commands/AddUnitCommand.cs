using System.Drawing;
using MapEditor.Entities;

namespace MapEditor.Commands
{
    public class AddUnitCommand : IReversableCommand
    {
        public CommandType Id { get; } = CommandType.AddUnit;

        public Point Point { get; set; }
        public Entity Unit { get; set; }
    }
}
