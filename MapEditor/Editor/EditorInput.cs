using System.Collections.Generic;
using System.Windows.Forms;
using MapEditor.Commands;
using MapEditor.Engine;

namespace MapEditor.Editor
{
    public class EditorInput : IInputController
    {
        private readonly MessageHub _messageHub;
        private readonly Camera _camera;
        private List<ICommand> Commands { get; set; } = new List<ICommand>();
        private int Index { get; set; }

        public EditorInput(MessageHub messageHub, Camera camera)
        {
            _messageHub = messageHub;
            _camera = camera;
        }

        public void OnKeyboardEvent(KeyPressEventArgs e)
        {
            //if ((Control.ModifierKeys & Keys.Shift) && (e.KeyChar & (char) Keys.Z) != 0)
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

        public void OnMouseEvent(MouseEventArgs e)
        {
            var direction = _camera.GetMoveDirection(e.Location);
            if (direction != Camera.CameraMotion.None &&
                direction != _camera.CurrentMotion)
            {
                Commands.Insert(Index, new MoveCameraCommand
                {
                    Direction = direction
                });
                _messageHub.Post(Commands[Index++]);
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
