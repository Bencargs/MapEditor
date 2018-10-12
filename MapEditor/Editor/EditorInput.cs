using System;
using System.Drawing;
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
        private Rectangle _selectionBox;
        private Point _startPoint;

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
                    _messageHub.Post(new UndoCommand());
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
                    if (_previousMouseState != ButtonState.LeftPressed)
                    {
                        _selectionBox = new Rectangle(e.Location.X, e.Location.Y, 1, 1);
                        _startPoint = e.Location;
                    }
                    else
                    {
                        _selectionBox.Width = Math.Abs(e.Location.X - _startPoint.X);
                        _selectionBox.Height = Math.Abs(e.Location.Y - _startPoint.Y);
                        _selectionBox.X = Math.Min(_startPoint.X, e.Location.X);
                        _selectionBox.Y = Math.Min(_startPoint.Y, e.Location.Y);
                    }
                    _messageHub.Post(new RenderSelectionCommand {Area = _selectionBox});
                    break;
                case ButtonState.LeftReleased:
                    if (_previousMouseState == ButtonState.LeftPressed)
                        _messageHub.Post(new SelectUnitsCommand { Area = _selectionBox});
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
