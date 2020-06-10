﻿using Common;
using System;

namespace MapEngine
{
    public class WpfGraphics : IGraphics
    {
        public int Width { get; }
        public int Height { get; }
        
        private readonly IImage _window;
        private byte[] _backBuffer;

        public WpfGraphics(IImage image)
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
            for (int y = 0; y < image.Height * 4; y+=4)
            {
                for (int x = 0; x < image.Width * 4; x += 4)
                {
                    var offset = Math.Max(0, (area.X * 4) + (area.Y * Width * 4));
                    var i = Math.Min(_backBuffer.Length - 4, x + (y * Width) + offset);
                    var colour = image[x, y];

                    _backBuffer[i] = colour.Red;
                    _backBuffer[i+1] = colour.Blue;
                    _backBuffer[i+2] = colour.Green;
                    _backBuffer[i+3] = colour.Alpha;
                }
            }
        }

        public void DrawLines(Colour colour, Point[] points)
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
