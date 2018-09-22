using MapEditor.Commands;

namespace MapEditor.Engine
{
    public interface IHandleCommand
    {
        void Handle(ICommand command);
        void Undo(ICommand command);
    }
}
