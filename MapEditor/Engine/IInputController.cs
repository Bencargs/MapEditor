using System.Drawing;
using System.Windows.Forms;

namespace MapEditor.Engine
{
    /// <summary>
    /// Allows seperation of input logic by context, eg editing and playing
    /// </summary>
    public interface IInputController
    {
        void OnKeyboardEvent(KeyPressEventArgs e);
        void OnMouseEvent(MouseState e);
    }

    public class MouseState
    {
        public Point Location { get; set; }
        public ButtonState State { get; set; }
    }

    public enum ButtonState
    {
        None = 0,
        LeftReleased = 1,
        LeftPressed = 2,
        RightReleased = 3,
        RightPressed = 4
    }
}
