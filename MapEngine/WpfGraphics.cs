using Common;
using System;
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
        private byte[] _backBuffer;

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
            throw new NotImplementedException();
        }

        public void DrawImage(IImage image, Rectangle area)
        {
            var maxWidth = Width * 4 - area.X * 4 - 1; // Why -1? god only knows..
            var maxHeight = Height * 4 - area.Y * 4;
            
            for (int y = 0; y < image.Height * 4; y+=4)
            {
                for (int x = 0; x < image.Width * 4; x+=4)
                {
                    if (x > maxWidth || y > maxHeight)
                        continue; // Dont draw outside the bounds

                    var offset = Math.Max(0, (area.X * 4) + (area.Y * Width * 4));
                    var i = Math.Min(_backBuffer.Length - 4, x + (y * Width) + offset);
                    var colour = image[x, y];
                    if (colour.Alpha == 0)
                        continue; // Dont draw something that's entirely transperant

                    _backBuffer[i] = colour.Red;
                    _backBuffer[i + 1] = colour.Blue;
                    _backBuffer[i + 2] = colour.Green;
                    _backBuffer[i + 3] = colour.Alpha;
                }
            }
        }

        public void DrawBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i+=4)
            {
                if (buffer[i + 3] == 0)
                    continue;

                _backBuffer[i] = buffer[i];
                _backBuffer[i + 1] = buffer[i + 1];
                _backBuffer[i + 2] = buffer[i + 2];
                _backBuffer[i + 3] = buffer[i + 3];
            }
        }

        public void DrawLines(Colour colour, Vector2[] points)
        {
            throw new NotImplementedException();
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
