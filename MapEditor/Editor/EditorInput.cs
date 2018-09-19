using System.Windows.Forms;
using MapEditor.Engine;

namespace MapEditor.Editor
{
    public class EditorInput : IInputController
    {
        private readonly MessageHub _messageHub;
        private readonly Camera _camera;

        public EditorInput(MessageHub messageHub, Camera camera)
        {
            _messageHub = messageHub;
            _camera = camera;
        }

        public void OnKeyboardEvent(KeyPressEventArgs e)
        {

        }

        public void OnMouseEvent(MouseEventArgs e)
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

            var button = e.Button;
            if (button == MouseButtons.Right)
            {
                // create and post command
            }
            else if (button == MouseButtons.Left)
            {

            }
        }
    }
}
