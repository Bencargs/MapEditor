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

        public void GetState(Point location, MouseButtons button)
        {
            Location = location;
            switch (button)
            {
                case MouseButtons.Left:
                    State = ButtonState.LeftPressed;
                    break;
                case MouseButtons.Right:
                    State = ButtonState.RightPressed;
                    break;
                default:
                    switch (State)
                    {
                        case ButtonState.LeftPressed:
                            State = ButtonState.LeftReleased;
                            break;
                        case ButtonState.RightPressed:
                            State = ButtonState.RightReleased;
                            break;
                        default:
                            State = ButtonState.None;
                            break;
                    }
                    break;
            }
        }
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
