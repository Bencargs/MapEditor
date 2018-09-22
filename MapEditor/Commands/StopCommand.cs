using MapEditor.Engine;

namespace MapEditor.Commands
{
    public class StopCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.Stop;
    }
}
