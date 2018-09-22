namespace MapEditor.Commands
{
    public class UndoCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.Undo;
    }
}
