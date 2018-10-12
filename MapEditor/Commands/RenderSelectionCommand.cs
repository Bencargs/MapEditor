using System.Drawing;

namespace MapEditor.Commands
{
    public class RenderSelectionCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.RenderSelection;
        public Rectangle Area { get; set; }
    }
}
