using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MapEditor.Engine
{
    public enum CommandType
    {
        None = 0,
        Move,
        Stop,
        Undo,
        Redo,
        PlaceTile,
        AddUnit,
        MoveCamera
    }

    public interface ICommand
    {
        CommandType Id { get; }
    }

    public class PlaceTileCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.PlaceTile;

        public Point Point { get; set; }
        public Terrain Terrain { get; set; }
        public List<Tile> PreviousTerrain { get; set; }    //todo: lazy initialize?
    }

    public class UndoCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.Undo;
    }

    public class RedoCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.Redo;
    }

    public interface IHandleCommand
    {
        void Handle(ICommand command);
        void Undo(ICommand command);
    }

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

                    var current = _commands[_index--];
                    NotifyUndo(current);
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
