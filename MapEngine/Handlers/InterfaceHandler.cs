using System;
using System.Numerics;
using Common;
using Common.Collision;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Extensions;
using MapEngine.Factories;
using MapEngine.Handlers.InputHandler;
using MapEngine.Services.Map;
using Rectangle = Common.Rectangle;

namespace MapEngine.Handlers
{
    public class InterfaceHandler
    {
        private readonly MapService _mapService;
        private readonly InputState _inputState;
        private readonly TextHandler _textHandler;
        private readonly MinimapHandler _minimapHandler;
        private readonly CursorHandler _cursorHandler;

        public InterfaceHandler(
            MapService mapService,
            InputState inputState,
            TextHandler textHandler,
            MinimapHandler minimapHandler,
            CursorHandler cursorHandler)
        {
            _mapService = mapService;
            _inputState = inputState;
            _textHandler = textHandler;
            _minimapHandler = minimapHandler;
            _cursorHandler = cursorHandler;
        }

        public void Initialise(string cursorFile, string fontPath)
        {
            FontFactory.LoadFonts(fontPath);
            _minimapHandler.Initialise();
            _cursorHandler.Initialise(cursorFile);
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            var buffer = new byte[_mapService.Width * _mapService.Height * 4]; // todo: dimentions from map

            DrawSelectionBox(buffer);
            DrawSelectedEntities(buffer);
            DrawHoverEntityStatus(buffer);
            DrawTextInput(buffer);
            _minimapHandler.Render(viewport, buffer);
            _cursorHandler.Render(viewport, graphics);

            graphics.DrawBytes(buffer, viewport);
        }

        private void DrawSelectionBox(byte[] buffer)
        {
            if (_inputState.SelectionStart is null) return;

            var width = _mapService.Width;
            var height = _mapService.Height;
            var startX = (int)Math.Min(_inputState.SelectionStart.Value.X, _inputState.Location.X).Clamp(0, width);
            var startY = (int)Math.Min(_inputState.SelectionStart.Value.Y, _inputState.Location.Y).Clamp(0, height);
            var endX = (int)Math.Max(_inputState.SelectionStart.Value.X, _inputState.Location.X).Clamp(0, width);
            var endY = (int)Math.Max(_inputState.SelectionStart.Value.Y, _inputState.Location.Y).Clamp(0, height);

            var bytesPerPixel = 4; // RGBA format
            var stride = width * bytesPerPixel;

            for (var y = startY; y < endY; y++)
            {
                for (var x = startX; x < endX; x++)
                {
                    var pixelIndex = (y * stride) + (x * bytesPerPixel);

                    if (x == startX || x == endX - 1 || y == startY || y == endY - 1)
                    {
                        // Highlight perimeter
                        buffer[pixelIndex] = 0; // Red component
                        buffer[pixelIndex + 1] = 255; // Green component
                        buffer[pixelIndex + 2] = 0; // Blue component
                        buffer[pixelIndex + 3] = 255; // Alpha component
                    }
                    else
                    {
                        // Fill interior
                        buffer[pixelIndex] = 255; // Red component
                        buffer[pixelIndex + 1] = 255; // Green component
                        buffer[pixelIndex + 2] = 255; // Blue component
                        buffer[pixelIndex + 3] = 25; // Alpha component
                    }
                }
            }
        }

        private void DrawSelectedEntities(byte[] buffer)
        {
            foreach (var selected in _inputState.SelectedEntities)
            {
                var bounds = selected.Bounds();
                DrawBounds(buffer, _mapService.Width, bounds);
            }
        }
        
        private void DrawHoverEntityStatus(byte[] buffer)
        {
            var hovered = _inputState.HoveredEntity;
            if (hovered == null) return;

            var unitComponent = hovered.GetComponent<UnitComponent>();
            var stateComponent = hovered.GetComponent<StateComponent>();
            if (stateComponent == null || unitComponent == null) return;

            if (!FontFactory.TryGetFont("default", out var font))
                return;

            // todo: some of this should be interface config
            var size = 13;
            var textColour = new Colour(215, 215, 230);
            var nameLocation = new Rectangle(_mapService.Width / 2, _mapService.Height - size * 2, _mapService.Width, _mapService.Height);
            var statusLocation = new Rectangle(_mapService.Width / 2, _mapService.Height - size, _mapService.Width, _mapService.Height);
            
            _textHandler.DrawText(buffer, unitComponent.Name, nameLocation, font, size, textColour, Justification.Center);
            _textHandler.DrawText(buffer, $"{stateComponent.CurrentState}", statusLocation, font, size, textColour, Justification.Center);
        }

        private void DrawTextInput(byte[] buffer)
        {
            if (!_inputState.IsTyping) return;
            
            if (!FontFactory.TryGetFont("default", out var font))
                return;

            var size = 13;
            var text = $"> {_inputState.TextInput}_";
            var textColour = new Colour(215, 215, 230);

            _textHandler.DrawText(buffer, text, new Rectangle(12, _mapService.Height - size, _mapService.Width, _mapService.Height), font, size, textColour);
        }
        
        private static void DrawBounds(
            byte[] buffer,
            int bufferWidth,
            Vector2[] bounds)
        {
            if (bounds == null || bounds.Length < 2)
                return;

            for (int i = 0; i < bounds.Length; i++)
            {
                var a = bounds[i];
                var b = bounds[(i + 1) % bounds.Length];

                DrawLine(buffer, bufferWidth, a, b);
            }
        }
        
        // todo: I'm stuck in a bresenheims repetition hell of my own design
        private static void DrawLine(
            byte[] buffer,
            int bufferWidth,
            Vector2 a,
            Vector2 b)
        {
            int x0 = (int)Math.Round(a.X);
            int y0 = (int)Math.Round(a.Y);
            int x1 = (int)Math.Round(b.X);
            int y1 = (int)Math.Round(b.Y);

            int dx = Math.Abs(x1 - x0);
            int dy = -Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            int stride = bufferWidth * 4;

            while (true)
            {
                int idx = y0 * stride + x0 * 4;
                if ((uint)idx < buffer.Length - 3)
                {
                    buffer[idx + 0] = 0;
                    buffer[idx + 1] = 255;
                    buffer[idx + 2] = 0;
                    buffer[idx + 3] = 255;
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}
