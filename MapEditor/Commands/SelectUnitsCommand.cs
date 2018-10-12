using System.Drawing;

namespace MapEditor.Commands
{
    public class SelectUnitsCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.SelectUnits;
        public Rectangle Area { get; set; }
    }
}
