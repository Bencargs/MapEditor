using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using Common;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Extensions;
using MapEngine.Services.Map;
using Rectangle = Common.Rectangle;

namespace MapEngine.Handlers
{
    public class InterfaceHandler
    {
        private readonly MapService _mapService;
        private readonly InputState _inputState;

        public InterfaceHandler(
            MapService mapService,
            InputState inputState)
        {
            _mapService = mapService;
            _inputState = inputState;
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            var buffer = new byte[_mapService.Width * _mapService.Height * 4]; // todo: dimentions from map

            DrawSelectionBox(buffer);
            DrawSelectedEntities(buffer);

            graphics.DrawBytes(buffer, viewport);
        }

        private void DrawSelectionBox(byte[] buffer)
        {
            if (_inputState.SelectionStart is null) return;

            var width = _mapService.Width;
            var startX = (int)Math.Min(_inputState.SelectionStart.Value.X, _inputState.Location.X).Clamp(0, width);
            var startY = (int)Math.Min(_inputState.SelectionStart.Value.Y, _inputState.Location.Y).Clamp(0, width);
            var endX = (int)Math.Max(_inputState.SelectionStart.Value.X, _inputState.Location.X).Clamp(0, width);
            var endY = (int)Math.Max(_inputState.SelectionStart.Value.Y, _inputState.Location.Y).Clamp(0, width);

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

        public void DrawSelectedEntities(byte[] buffer)
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

        public static void DrawBoxOnImage(Vector2 location, int width, int height, float facingAngle, byte[] image, int imageWidth)
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
                    if (x == startX || x == endX - 1 || y == startY || y == endY - 1)
                    {
                        Vector2 rotatedPoint = RotatePoint(new Vector2(x, y), new Vector2(centerX, centerY), facingAngle);
                        int pixelIndex = ((int)rotatedPoint.Y * stride) + ((int)rotatedPoint.X * bytesPerPixel);
                        if (pixelIndex < 0) continue;

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
