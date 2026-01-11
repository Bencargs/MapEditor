using System;
using System.Numerics;
using Common;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Extensions;
using MapEngine.Factories;
using MapEngine.Services.Map;
using Rectangle = Common.Rectangle;

namespace MapEngine.Handlers
{
    public class InterfaceHandler
    {
        private readonly MapService _mapService;
        private readonly InputState _inputState;
        private readonly TextHandler _textHandler;
        private readonly CursorHandler _cursorHandler;

        public InterfaceHandler(
            MapService mapService,
            InputState inputState,
            TextHandler textHandler,
            CursorHandler cursorHandler)
        {
            _mapService = mapService;
            _inputState = inputState;
            _textHandler = textHandler;
            _cursorHandler = cursorHandler;
        }

        public void Initialise(string cursorFile, string fontPath)
        {
            FontFactory.LoadFonts(fontPath);
            _cursorHandler.Initialise(cursorFile);
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            var buffer = new byte[_mapService.Width * _mapService.Height * 4]; // todo: dimentions from map

            DrawSelectionBox(buffer);
            DrawSelectedEntities(buffer);
            DrawHoverEntityStatus(buffer);
            DrawTextInput(buffer);
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
                var angle = selected.GetComponent<LocationComponent>().FacingAngle;
                var area = selected.Texture();
                var location = selected.Location();
                var centeredLocation = new Vector2(location.X - (area.Width / 2), location.Y - (area.Height / 2));
                DrawBoxOnImage(centeredLocation, area.Width + 2, area.Height + 2, angle, buffer, _mapService.Width);
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

        private static void DrawBoxOnImage(Vector2 location, int width, int height, float facingAngle, byte[] image, int imageWidth)
        {
            int startX = (int)location.X;
            int startY = (int)location.Y;
            int endX = startX + width;
            int endY = startY + height;

            // Calculate center point of the box
            float centerX = startX + (width / 2f);
            float centerY = startY + (height / 2f);

            int bytesPerPixel = 4; // RGBA format
            int stride = imageWidth * bytesPerPixel;

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (x == startX || x == endX || y == startY || y == endY)
                    {
                        Vector2 rotatedPoint = RotatePoint(new Vector2(x, y), new Vector2(centerX, centerY), facingAngle);
                        int pixelIndex = ((int)rotatedPoint.Y * stride) + ((int)rotatedPoint.X * bytesPerPixel);
                        if (pixelIndex < 0 || pixelIndex > image.Length - 1) continue;

                        image[pixelIndex] = 0;       // Red component
                        image[pixelIndex + 1] = 255; // Green component
                        image[pixelIndex + 2] = 0;   // Blue component
                        image[pixelIndex + 3] = 255; // Alpha component
                    }
                }
            }
        }

        private static Vector2 RotatePoint(Vector2 point, Vector2 center, float angle)
        {
            float radians = angle.ToRadians();
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            float translatedX = point.X - center.X;
            float translatedY = point.Y - center.Y;

            float rotatedX = (translatedX * cos) - (translatedY * sin);
            float rotatedY = (translatedX * sin) + (translatedY * cos);

            return new Vector2(rotatedX + center.X, rotatedY + center.Y);
        }
    }
}
