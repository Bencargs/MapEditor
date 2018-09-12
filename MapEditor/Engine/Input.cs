using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapEditor.Engine
{
    /// <summary>
    /// Allows seperation of input logic by context, eg editing and playing
    /// </summary>
    public interface IInputController
    {
        void OnKeyboardEvent(KeyPressEventArgs e);
        void OnMouseEvent(MouseEventArgs e);
    }
}
