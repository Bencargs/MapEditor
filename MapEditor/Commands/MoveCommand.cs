using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.Commands
{
    public interface ICommand
    { }

    public class MoveCommand : ICommand
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsQueued { get; set; }
    }

    public class StopCommand : ICommand
    {
    }
}
