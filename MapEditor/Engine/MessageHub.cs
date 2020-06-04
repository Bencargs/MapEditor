using System.Collections.Generic;
using System.Linq;
using MapEditor.Commands;

namespace MapEditor.Engine
{
    public class MessageHub
    {
        private readonly Dictionary<CommandType, List<IHandleCommand>> _subscribers =
            new Dictionary<CommandType, List<IHandleCommand>>();

        private const int QueueSize = 100;
        private readonly List<ICommand> _commands = new List<ICommand>(QueueSize);
        private int _index;

        public void Post(ICommand command)
        {
            switch (command.Id)
            {
                case CommandType.Undo:
                {
                    if (_index <= 0)
                        return;

                    var reversableCommand = _commands.OfType<IReversableCommand>().LastOrDefault();
                    if (reversableCommand != null)
                    {
                        _index = _commands.LastIndexOf(reversableCommand);
                        NotifyUndo(reversableCommand);
                    }
                    break;
                }
                case CommandType.Redo:
                {
                    if (_commands.Count <= _index)
                        return;

                    var current = _commands[_index++];
                    Notify(current);
                    break;
                }
                default:
                    //_commands.RemoveRange(_index, _commands.Count);
                    _commands.Insert(_index++, command);
                    Notify(command);
                    break;
            }
        }

        public void Subscribe(IHandleCommand handler, CommandType commandId)
        {
            if (_subscribers.ContainsKey(commandId))
                _subscribers[commandId].Add(handler);
            else
                _subscribers[commandId] = new List<IHandleCommand> { handler };
        }

        // todo: seperate notify from post, so commands can be send, and distrubuted asynchronously
        public void Notify(ICommand command)
        {
            if (!_subscribers.TryGetValue(command.Id, out List<IHandleCommand> handlers))
                return;

            foreach (var h in handlers)
            {
                h.Handle(command);
            }
        }

        public void NotifyUndo<T>(T command)
            where T : ICommand
        {
            if (!_subscribers.TryGetValue(command.Id, out List<IHandleCommand> handlers))
                return;

            foreach (var h in handlers)
            {
                h.Undo(command);
            }
        }
    }
}
