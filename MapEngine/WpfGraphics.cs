using Common;
using System;
using System.Numerics;
using System.Windows.Media.Imaging;
using MapEngine.Extensions;

namespace MapEngine
{
    public class WpfGraphics : IGraphics
    {
        public int Width { get; }
        public int Height { get; }
        public WriteableBitmap Bitmap;

        private readonly byte[] _backBuffer;

        public WpfGraphics(int width, int height)
        {
            Width = width;
            Height = height;

            Bitmap = BitmapFactory.New(Width, Height);
            _backBuffer = Bitmap.ToByteArray();
        }

        public void Clear()
        {
            Bitmap.Clear();
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
                    var colour = image[x, y];
                    if (colour.Alpha == 0)
                        continue; // Dont draw something that's entirely transperant

                    if (colour.Alpha != 255) // If there's an alpha component, blend with background
                        colour = BlendColour(x, y, colour);

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

                var opacity = buffer[i + 3] / 255f;
                // todo: keep previous pixel operations? averaging washes out the image
                _backBuffer[k + 0] = MergePixel(_backBuffer[k + 0], buffer[i + 0], opacity);
                _backBuffer[k + 1] = MergePixel(_backBuffer[k + 1], buffer[i + 1], opacity);
                _backBuffer[k + 2] = MergePixel(_backBuffer[k + 2], buffer[i + 2], opacity);
                _backBuffer[k + 3] = 255;
            }
        }

        public void Desaturate(float[] buffer, Rectangle area)
        {
            for (int i = 0; i < buffer.Length; i ++)
            {
                var amount = buffer[i];
                if (amount == 0f) continue;

                int x = i % Width;
                int y = i / Width;
                
                var original = GetPixel(x, y);

                var newColour = new Colour(
                    (byte)(original.Red - 255 * amount).Clamp(0, 255),
                    (byte)(original.Blue - 255 * amount).Clamp(0, 255),
                    (byte)(original.Green - 255 * amount).Clamp(0, 255),
                    original.Alpha);

                SetPixel(x, y, newColour);
            }
        }

        // todo: merge with BlendColour
        private byte MergePixel(byte a, byte b, float alpha)
        {
            var reciprical = 1 - alpha;

            var c = (a * reciprical) + (b * alpha);

            return (byte)c;
        }

        private void SetPixel(int x, int y, Colour colour)
        {
            var index = x * 4 + y * 4 * Width;

            _backBuffer[index] = colour.Red;
            _backBuffer[index + 1] = colour.Blue;
            _backBuffer[index + 2] = colour.Green;
            _backBuffer[index + 3] = colour.Alpha;
        }

        private Colour GetPixel(int x, int y)
        {
            var index = x * 4 + y * 4 * Width;

            var red = _backBuffer[index];
            var blue = _backBuffer[index + 1];
            var green = _backBuffer[index + 2];
            var alpha = _backBuffer[index + 3];

            return new Colour(red, blue, green, alpha);
        }

        private Colour BlendColour(int x, int y, Colour colour)
        {
            var original = GetPixel(x, y);
            var blend = (float)colour.Alpha / 255;
            var reciprocal = 1 - blend;

            return new Colour(
                (byte)(colour.Red * blend + original.Red * reciprocal),
                (byte)(colour.Blue * blend + original.Blue * reciprocal),
                (byte)(colour.Green * blend + original.Green * reciprocal),
                255);
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
            Bitmap.FromByteArray(_backBuffer);
        }
    }
}
