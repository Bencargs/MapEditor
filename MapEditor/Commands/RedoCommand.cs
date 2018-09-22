namespace MapEditor.Commands
{
    public class RedoCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.Redo;
    }
}
