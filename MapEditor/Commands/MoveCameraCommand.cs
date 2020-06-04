using MapEditor.Engine;
using MapEditor.Handlers;

namespace MapEditor.Commands
{
    public class MoveCameraCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.MoveCamera;
        public CameraHandler.CameraMotion Direction { get; set; }
    }
}
