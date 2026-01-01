using System;
using System.Collections.Generic;
using System.Linq;
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
        public Entity? HoveredEntity { get; set; }
        public Vector2? SelectionStart { get; set; }
        public Command CurrentCommand = Command.None;
        public readonly List<Entity> SelectedEntities = new List<Entity>();
        
        public bool IsTyping { get; set; }
        public string TextInput { get; set; }
        
        public enum Command
        {
            None,
            Stop,
            Move,
            Attack,
            Load,
            Unload,
            Guard,
            Patrol
        }
    }

    public class InputHandler
        : IHandleCommand<CreateEntityCommand>
    {
        private readonly InputState _inputState;
        private readonly MessageHub _messageHub;
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly Dictionary<Key, ICommandStrategy> _commandBindings;

        private ICommandStrategy? _commandStrategy = null;
        
        public InputHandler(
            InputState inputState,
            MessageHub messageHub,
            UnloadCommandStrategy unloadCommandStrategy)
        {
            _inputState = inputState;
            _messageHub = messageHub;
            _commandBindings = new Dictionary<Key, ICommandStrategy>
            {
                [Key.U] = unloadCommandStrategy,
            };
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
            if (_commandStrategy != null)
            {
                var command = _commandStrategy.CreateCommand(currentLocation, _inputState.SelectedEntities);
                _messageHub.Post(command);
                
                _commandStrategy = null;
                _inputState.SelectionStart = null;
                _inputState.CurrentCommand = InputState.Command.None;
                return;
            }
            
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

            var selectedEntities = _entities
                .Where(entity => entity.BelongsTo(Constants.PlayerTeam))
                .Where(entity => entity.Hitbox().HasCollided(selectionArea))
                .OrderBy(entity => Vector2.Distance(selectionArea.Location, entity.Location()));
            
            _inputState.SelectedEntities.Clear();
            foreach (var entity in selectedEntities)
            { 
                _inputState.SelectedEntities.Add(entity);
                if (width == 0 && height == 0)
                {
                    // eg if single click, not selection box drag
                    // todo: there's probably a better way to do this
                    break;
                }
            }

            _inputState.SelectionStart = null;
        }

        public void HandleMouseMove(Vector2 location)
        {
            if (location == _inputState.Location)
                return;
            
            _inputState.Location = location;

            var area = new BoundingCircle { Radius = 2, Location = location };
            _inputState.HoveredEntity = _entities
                .Where(entity => entity.Hitbox().HasCollided(area))
                .OrderBy(entity => Vector2.Distance(area.Location, entity.Location()))
                .FirstOrDefault();
        }

        public void HandleKeyDown(Key key)
        {
            // todo: move to a TextHandler class?
            if (_inputState.IsTyping)
            {
                switch (key)
                {
                    case Key.Enter:
                        CommitTypedText();
                        return;

                    case Key.Escape:
                        _inputState.IsTyping = false;
                        _inputState.TextInput = "";
                        return;

                    case Key.Back:
                        if (!string.IsNullOrEmpty(_inputState.TextInput))
                            _inputState.TextInput = _inputState.TextInput.Substring(0, _inputState.TextInput.Length - 1);
                        return;

                    case Key.Space:
                        _inputState.TextInput += " ";
                        return;

                    // Ignore everything else here – characters come via HandleTextInput
                    default:
                        return;
                }
            }
            
            // start typing
            if (key == Key.Enter)
            {
                _inputState.IsTyping = true;
                _inputState.TextInput = "";
                return;
            }
            
            if (!_commandBindings.TryGetValue(key, out var commandStrategy))
            {
                _commandStrategy = null;
                _inputState.CurrentCommand = InputState.Command.None;
                return;
            }

            var applicableUnits = _inputState.SelectedEntities
                .Where(commandStrategy.IsApplicable)
                .ToList();
            if (!applicableUnits.Any())
            {
                _commandStrategy = null;
                _inputState.CurrentCommand = InputState.Command.None;
                return;
            }
            
            _inputState.SelectedEntities.Clear();
            _inputState.SelectedEntities.AddRange(applicableUnits);
            
            _commandStrategy = commandStrategy;
            _inputState.CurrentCommand = commandStrategy.CommandType;

            // var command = InputState.Command.None;
            // List<Entity> applicableUnits = new List<Entity>();
            // switch (key)
            // {
            //     case Key.U:
            //         command = InputState.Command.Unload;
            //         applicableUnits = _inputState.SelectedEntities.Where(x => x.GetComponents<CargoComponent>().Any()).ToList();
            //         break;
            //     default:
            //         _inputState.CurrentCommand = InputState.Command.None;
            //         break;
            // }
            //
            // if (!applicableUnits.Any())
            // {
            //     _inputState.CurrentCommand = InputState.Command.None;
            //     return;
            // }
            //
            // _inputState.CurrentCommand = command;
            // _inputState.SelectedEntities.Clear();
            // _inputState.SelectedEntities.AddRange(applicableUnits);
        }
        
        public void HandleTextInput(string text)
        {
            if (!_inputState.IsTyping) return;

            _inputState.TextInput += text;
        }

        public void Handle(CreateEntityCommand command)
        {
            if (command.Entity.GetComponent<UnitComponent>() is null)
                return;

            _entities.Add(command.Entity);
        }
        
        private void CommitTypedText()
        {
            var text = _inputState.TextInput.Trim();
            _inputState.IsTyping = false;
            _inputState.TextInput = "";

            if (string.IsNullOrWhiteSpace(text))
                return;
            
            _messageHub.Post(new TextCommand { Text = text });
        }
    }
}
