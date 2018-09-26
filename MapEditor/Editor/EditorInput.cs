using System.Windows.Forms;
using MapEditor.Commands;
using MapEditor.Engine;
using ButtonState = MapEditor.Engine.ButtonState;

namespace MapEditor.Editor
{
    public class EditorInput : IInputController
    {
        private readonly MessageHub _messageHub;
        private readonly Camera _camera;
        private ButtonState _previousMouseState;

        public EditorInput(MessageHub messageHub, Camera camera)
        {
            _messageHub = messageHub;
            _camera = camera;
        }

        public void OnKeyboardEvent(KeyPressEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                if ((e.KeyChar & (char) Keys.Z) != 0)
                {
                    //_messageHub.Post(new UndoCommand
                    //{
                        
                    //})
                }
                else if ((e.KeyChar & (char) Keys.Z) != 0)
                {

                }
            }
        }

        public void OnMouseEvent(MouseState e)
        {
            var direction = _camera.GetMoveDirection(e.Location);
            if (direction != Camera.CameraMotion.None &&
                direction != _camera.CurrentMotion)
            {
                _messageHub.Post(new MoveCameraCommand
                {
                    Direction = direction
                });
            }

            switch (e.State)
            {
                case ButtonState.None:
                    break;
                case ButtonState.LeftPressed:
                    //Post StartSelectionCommand {e.Location}
                    break;
                case ButtonState.LeftReleased:
                    if (_previousMouseState == ButtonState.LeftPressed)
                    {
                        //Post CompleteSelectionCommand {e.Location}
                    }
                    break;
                case ButtonState.RightReleased:
                    break;
                case ButtonState.RightPressed:
                    // Post a command
                    break;
            }
            _previousMouseState = e.State;
        }
    }
}
