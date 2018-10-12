using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.Commands
{
    public class CreateCameraCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.CreateCamera;
        public Rectangle Viewport { get; set; }
    }
}
