using System.Drawing;
using MapEditor.Commands;
using MapEditor.Common;
using MapEditor.Engine;
using MapEditor.Handlers.CollisionHandler;

namespace MapEditor.Handlers
{
    public class CameraHandler : IHandleCommand
    {
        private readonly MessageHub _messageHub;
        private Rectangle _viewport;
        private Rectangle _innerViewport;
        private const int MoveSpeed = 10;   //todo: this should be editable via settings
        public CameraMotion CurrentMotion { get; set; }

        //[Flags] todo: investigate movement in two simaltaneous directions
        public enum CameraMotion
        {
            None,
            Stop,
            Up,
            Down,
            Left,
            Right
        }

        //todo: replace parameter with service locator
        public CameraHandler(MessageHub messageHub, Point position, int width, int height)
        {
            _messageHub = messageHub;
            var innerViewportOffset = 30;
            _viewport = new Rectangle(position, new Size(width, height));
            _innerViewport = new Rectangle(_viewport.X + innerViewportOffset,
                                           _viewport.Y + innerViewportOffset,
                                           _viewport.Width - innerViewportOffset,
                                           _viewport.Height - innerViewportOffset);
        }

        public void Init()
        {
            _messageHub.Post(new CreateCameraCommand
            {
                Viewport = _viewport
            });
            _messageHub.Subscribe(this, CommandType.MoveCamera);
        }

        public CameraMotion GetMoveDirection(Point point)
        {
            var direction = CameraMotion.None;
            if (point.X < _innerViewport.Left)
                direction = CameraMotion.Left;
            else if (point.X > _innerViewport.Right)
                direction = CameraMotion.Right;
            else if (point.Y < _innerViewport.Top)
                direction = CameraMotion.Up;
            else if (point.Y > _innerViewport.Bottom)
                direction = CameraMotion.Down;
            return direction;
        }

        public bool Contains(ICollider collider)
        {
            var boundingBox = new BoundingBox
            {
                Position = _viewport.Location,
                Height = _viewport.Height,
                Width = _viewport.Width
            };
            return boundingBox.IsCollided(collider);
        }

        public void Handle(ICommand command)
        {
            switch (command)
            {
                case MoveCameraCommand moveCommand:
                    CurrentMotion = moveCommand.Direction;
                    break;
            }
        }

        public Matrix GetTransformations()
        {
            //var translationMatrix = Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
            //var rotationMatrix = Matrix.CreateRotationZ(Rotation);
            //var scaleMatrix = Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));
            //var originMatrix = Matrix.CreateTranslation(new Vector3(Origin.X, Origin.Y, 0));

            //return translationMatrix * rotationMatrix * scaleMatrix * originMatrix;

            //-- Game ->
            //var screenScale = GetScreenScale();
            //var viewMatrix = CameraHandler.GetTransform();

            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
            //    null, null, null, null, viewMatrix * Matrix.CreateScale(screenScale));

            return Matrix.CreateTranslation(new Vector3(-_viewport.X, -_viewport.Y, 0));
        }

        //todo: ensure screen is scaled to correct resolution - map class?
        //public Vector3 GetScreenScale()
        //{
        //    var scaleX = (float)_graphicsDevice.Viewport.Width / (float)_width;
        //    var scaleY = (float)_graphicsDevice.Viewport.Height / (float)_height;
        //    return new Vector3(scaleX, scaleY, 1.0f);
        //}

        public void Update(/*double elapsed*/) //todo: implement elapsed
        {
            switch (CurrentMotion)
            {
                case CameraMotion.Stop:
                case CameraMotion.None:
                    break;
                case CameraMotion.Up:
                    _viewport.X -= MoveSpeed;
                    break;
                case CameraMotion.Down:
                    _viewport.Y -= MoveSpeed;
                    break;
                case CameraMotion.Left:
                    _viewport.X += MoveSpeed;
                    break;
                case CameraMotion.Right:
                    _viewport.Y += MoveSpeed;
                    break;
            }
        }

        public void Undo(ICommand command)
        {
            // No op
        }
    }
}
