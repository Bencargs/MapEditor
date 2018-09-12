using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapEditor.Engine;

namespace MapEditor.Commands
{
    public class MoveCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.Move;
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsQueued { get; set; }
    }

    public class StopCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.Stop;
    }
}
