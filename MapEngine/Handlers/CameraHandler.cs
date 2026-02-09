using System;
using System.Numerics;
using Common;
using MapEngine.Handlers.InputHandler;
using MapEngine.ResourceLoading;
using MapEngine.Services.Map;

namespace MapEngine.Handlers
{
    public class CameraHandler
    {
        private readonly InputState _inputState;
        private readonly MapService _mapService;
        private Camera _camera;
        private const int _viewportOffset = 30; //todo: camera settings?
        private const int _moveSpeed = 20;
        
        public CameraHandler(
            InputState inputState,
            MapService mapService)
        {
            _inputState = inputState;
            _mapService = mapService;
        }

        public void Initialise(string mapFilename)
        {
            _camera = CameraLoader.LoadCamera(mapFilename);
            _camera.Viewport = new Rectangle(
                _camera.Viewport.X,
                _camera.Viewport.Y ,
                _camera.Viewport.Width,
                _camera.Viewport.Height);
        }

        public void Update()
        {
            // todo - move to a method in the camera class
            var innerWidth = Math.Max(0, _camera.Viewport.Width - (_viewportOffset * 2));
            var innerHeight = Math.Max(0, _camera.Viewport.Height - (_viewportOffset * 2));
            var innerViewport = new Rectangle(_viewportOffset, _viewportOffset, innerWidth, innerHeight);
            
            // Only move the camera if the mouse is near the window bounds
            if (innerViewport.Contains(_inputState.Location))
                return;
            
            if (_inputState.Location.X < innerViewport.X)
                _camera.Viewport.X -= _moveSpeed;
            else if (_inputState.Location.X > innerViewport.X + innerViewport.Width)
                _camera.Viewport.X += _moveSpeed;
            if (_inputState.Location.Y < innerViewport.Y)
                _camera.Viewport.Y -= _moveSpeed;
            else if (_inputState.Location.Y > innerViewport.Y + innerViewport.Height)
                _camera.Viewport.Y += _moveSpeed;
            
            ClampViewport();
        }

        public Rectangle GetViewport() => _camera.Viewport;
        
        public Vector2 ScreenToWorld(Vector2 location)
        {
            return new Vector2(location.X + _camera.Viewport.X, location.Y + _camera.Viewport.Y);
        }
        
        public void Target(Vector2 location)
        {
            _camera.Viewport.X = (int)Math.Round(location.X - (_camera.Viewport.Width / 2f));
            _camera.Viewport.Y = (int)Math.Round(location.Y - (_camera.Viewport.Height / 2f));
            ClampViewport();
        }
        
        private void ClampViewport()
        {
            var maxX = Math.Max(0, _mapService.Width - _camera.Viewport.Width);
            var maxY = Math.Max(0, _mapService.Height - _camera.Viewport.Height);
            _camera.Viewport.X = Math.Max(0, Math.Min(_camera.Viewport.X, maxX));
            _camera.Viewport.Y = Math.Max(0, Math.Min(_camera.Viewport.Y, maxY));
        }
    }
}
