using Common;
using System;
using System.Linq;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace MapEngine
{
    public class WpfGraphics : IGraphics
    {
        public int Width { get; }
        public int Height { get; }
        public WriteableBitmap Bitmap => _window.Bitmap;
        
        private readonly WpfImage _window;
        private readonly byte[] _backBuffer;

        public WpfGraphics(WpfImage image)
        {
            Width = image.Width;
            Height = image.Height;

            _window = image;
            _backBuffer = new byte[Width * Height * 4];
        }

        public void Clear()
        {
            for (var i = 0; i < _backBuffer.Length; i += 4)
            {
                _backBuffer[i] = 0;
                _backBuffer[i + 1] = 0;
                _backBuffer[i + 2] = 0;
                _backBuffer[i + 3] = 0;
            }
        }

        public void DrawCircle(Colour colour, Rectangle area)
        {
            var radius = Math.Min(area.Height, area.Width);
            var centerX = area.X;
            var centerY = area.Y;
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var point = (int) (Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));
                    var perimeter = (int) Math.Sqrt(point);
                    if (radius == perimeter)
                    {
                        SetPixel(x, y, colour);
                    }
                }
            }
        }

        public void DrawImage(IImage image, Rectangle area)
        {
            // restrict drawing to map bounds
            var minX = Math.Max(0, 0 - area.X);
            var minY = Math.Max(0, 0 - area.Y);
            var maxX = Math.Min(image.Width, area.Width);
            var maxY = Math.Min(image.Height, area.Height);

            for (var x = minX; x < maxX && x + area.X < Width - 1; x++)
            {
                for (var y = minY; y < maxY && y + area.Y < Height - 1; y++)
                {
                    var colour = image[x * 4, y * 4];
                    if (colour.Alpha == 0)
                        continue; // Dont draw something that's entirely transperant

                    SetPixel(x + area.X, y + area.Y, colour);
                }
            }
        }

        public void DrawBytes(byte[] buffer, Rectangle area)
        {
            //todo: array.copy?
            for (int i = 0; i < buffer.Length; i += 4)
            {
                if (buffer[i + 3] == 0)
                    continue;

                var k = i + (area.X * 4) + (area.Y * 4 * area.Width);
                if (k > _backBuffer.Length || k < 0)
                    continue;

                _backBuffer[k] = buffer[i];
                _backBuffer[k + 1] = buffer[i + 1];
                _backBuffer[k + 2] = buffer[i + 2];
                _backBuffer[k + 3] = buffer[i + 3];
            }
        }

        private void SetPixel(int x, int y, Colour colour)
        {
            var index = x * 4 + y * 4 * Width;

            _backBuffer[index] = colour.Red;
            _backBuffer[index + 1] = colour.Blue;
            _backBuffer[index + 2] = colour.Green;
            _backBuffer[index + 3] = colour.Alpha;
        }

        public void DrawLines(Colour colour, Vector2[] points)
        {
            var p1 = points[0];
            var p2 = points[1];

            // via Bresenham's
            var dx = Math.Abs(p2.X - p1.X);
            var dy = Math.Abs(p2.Y - p1.Y);
            
            var sx = p1.X < p2.X ? 1 : -1;
            var sy = p1.Y < p2.Y ? 1 : -1;
            var err = (dx > dy ? dx : -dy) / 2;
            var tolerance = 0.001;

            for (int j = 0; j < 50; j++)
            {
                if (p1.X > 0 && p1.Y > 0 && p1.X < Width - 1 && p1.Y < Height - 1)
                    SetPixel((int)p1.X, (int)p1.Y, colour);
                
                if (Math.Abs(p1.X - p2.X) < tolerance && 
                    Math.Abs(p1.Y - p2.Y) < tolerance)
                    break;

                var e2 = err;
                if (e2 > -dx)
                {
                    err -= dy;
                    p1.X += sx;
                }

                if (e2 < dy)
                {
                    err += dx;
                    p1.Y += sy;
                }
            }
        }

        public void DrawRectangle(Colour colour, Rectangle area)
        {
            throw new NotImplementedException();
        }

        public void FillRectangle(Colour colour, Rectangle area)
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            _window.Draw(_backBuffer);
        }
    }
}
