using MapEditor.Engine;

namespace MapEditor.Commands
{
    public class MoveCameraCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.MoveCamera;
        public Camera.CameraMotion Direction { get; set; }
    }
}
