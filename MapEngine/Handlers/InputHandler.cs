using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Collision;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;

namespace MapEngine.Handlers
{
    public class InputState
    {
        public Vector2 Location { get; set; }
        public Vector2? SelectionStart { get; set; }
        public readonly List<Entity> SelectedEntities = new List<Entity>();
    }

    public class InputHandler
        : IHandleCommand<CreateEntityCommand>
    {
        private readonly InputState _inputState;
        private readonly MessageHub _messageHub;
        private readonly List<Entity> _entities = new List<Entity>();

        public InputHandler(
            InputState inputState,
            MessageHub messageHub)
        {
            _inputState = inputState;
            _messageHub = messageHub;
        }

        public Vector2 GetMouseLocation(MouseEventArgs args, Image image)
        {
            var point = args.GetPosition(image);
            var renderedImageSize = new Size(image.ActualWidth, image.ActualHeight);
            var originalImageSize = new Size(image.Source.Width, image.Source.Height);

            var relativePoint = new Point(point.X / renderedImageSize.Width, point.Y / renderedImageSize.Height);
            var imagePoint = new Point(relativePoint.X * originalImageSize.Width, relativePoint.Y * originalImageSize.Height);

            return new Vector2((float)imagePoint.X, (float)imagePoint.Y);
        }

        public void HandleLeftMouseDown(Vector2 location)
        {
            _inputState.SelectionStart = location;
        }

        public void HandleRightMouseDown(Vector2 location)
        {
            if (_inputState.SelectedEntities.Count == 0) return;

            var moveCommand = new MoveCommand
            {
                Entities = _inputState.SelectedEntities,
                Destination = location,
                MovementMode = MovementMode.Seek,
                Queue = false, // todo: check of shift is down
            };
            _messageHub.Post(moveCommand);
        }

        public void HandleLeftMouseUp(Vector2 currentLocation)
        {
            if (_inputState.SelectionStart is null) return;

            // todo: this will need to convert screen to map - eg. mouseLocation + cameraLocation
            var x = (int)Math.Min(_inputState.SelectionStart.Value.X, currentLocation.X);
            var y = (int)Math.Min(_inputState.SelectionStart.Value.Y, currentLocation.Y);
            var width = (int)Math.Abs(currentLocation.X - _inputState.SelectionStart.Value.X);
            var height = (int)Math.Abs(currentLocation.Y - _inputState.SelectionStart.Value.Y);
            var selectionArea = new BoundingBox
            {
                Location = new Vector2(x, y),
                Width = width,
                Height = height
            };

            _inputState.SelectedEntities.Clear();
            foreach (var entity in _entities)
            {
                if (!entity.BelongsTo(Constants.PlayerTeam) ||
                    !entity.Hitbox().HasCollided(selectionArea))
                    continue;

                _inputState.SelectedEntities.Add(entity);
            }

            _inputState.SelectionStart = null;
        }

        public void HandleMouseMove(Vector2 location)
        {
            _inputState.Location = location;
            //_cameraHandler.GetViewport()
            //IGraphics _graphics
            //_2dRenderer.DrawLayer(); / /cant draw directly, missing dependencies
            //shared dependency?
            //message passing
            // todo: mouse over logic
            // render
        }

        public void Handle(CreateEntityCommand command)
        {
            if (command.Entity.GetComponent<UnitComponent>() is null)
                return;

            _entities.Add(command.Entity);
        }
    }
}
