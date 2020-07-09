using Common;
using MapEngine.ResourceLoading;

namespace MapEngine.Handlers
{
    public class CameraHandler
    {
        private Camera _camera;
        private const int _viewportOffset = 30; //todo: camera settings?
        private const int _moveSpeed = 2;

        public void Initialise(string mapFilename)
        {
            _camera = CameraLoader.LoadCamera(mapFilename);
            _camera.InnerViewport = new Rectangle(_camera.Viewport.X + _viewportOffset,
                                                  _camera.Viewport.Y + _viewportOffset,
                                                  _camera.Viewport.Width - _viewportOffset,
                                                  _camera.Viewport.Height - _viewportOffset);
        }

        public void Update()
        {
            // Move the camera if the mouse is near the window bounds
            if (_camera.Viewport.Contains(Mouse.Location))
            {
                if (Mouse.Location.X < _camera.InnerViewport.X)
                    _camera.Viewport.X -= _moveSpeed;
                else if (Mouse.Location.X > _camera.InnerViewport.Width)
                    _camera.Viewport.X += _moveSpeed;
                if (Mouse.Location.Y < _camera.InnerViewport.Y)
                    _camera.Viewport.Y -= _moveSpeed;
                else if (Mouse.Location.X > _camera.InnerViewport.Height)
                    _camera.Viewport.Y += _moveSpeed;
            }
        }

        public Rectangle GetViewport() => _camera.Viewport;
    }
}
