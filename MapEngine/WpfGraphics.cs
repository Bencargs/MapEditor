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
            Array.Clear(_backBuffer, 0, _backBuffer.Length);
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
            int destX0 = Math.Max(0, area.X);
            int destY0 = Math.Max(0, area.Y);
            int destX1 = Math.Min(Width,  area.X + area.Width);
            int destY1 = Math.Min(Height, area.Y + area.Height);

            int copyW = destX1 - destX0;
            int copyH = destY1 - destY0;
            if (copyW <= 0 || copyH <= 0) return;

            int srcX0 = destX0 - area.X;
            int srcY0 = destY0 - area.Y;

            copyW = Math.Min(copyW, image.Width  - srcX0);
            copyH = Math.Min(copyH, image.Height - srcY0);
            if (copyW <= 0 || copyH <= 0) return;

            const int bpp = 4;
            int srcStride = image.Width * bpp;
            int dstStride = Width * bpp;

            for (int row = 0; row < copyH; row++)
            {
                int sRow = (srcY0 + row) * srcStride + srcX0 * bpp;
                int dRow = (destY0 + row) * dstStride + destX0 * bpp;

                for (int col = 0; col < copyW; col++)
                {
                    int s = sRow + col * bpp;
                    int d = dRow + col * bpp;

                    // Dont draw something that's entirely transperant
                    byte alpha = image.Buffer[s + 3];
                    if (alpha == 0)
                        continue;

                    if (alpha == 255)
                    {
                        // copy pixel
                        _backBuffer[d + 0] = image.Buffer[s + 0];
                        _backBuffer[d + 1] = image.Buffer[s + 1];
                        _backBuffer[d + 2] = image.Buffer[s + 2];
                        _backBuffer[d + 3] = 255;
                        continue;
                    }

                    int inv = 255 - alpha;

                    // [0]=Red, [1]=Blue, [2]=Green, [3]=Alpha
                    _backBuffer[d + 0] = (byte)((image.Buffer[s + 0] * alpha + _backBuffer[d + 0] * inv + 127) / 255);
                    _backBuffer[d + 1] = (byte)((image.Buffer[s + 1] * alpha + _backBuffer[d + 1] * inv + 127) / 255);
                    _backBuffer[d + 2] = (byte)((image.Buffer[s + 2] * alpha + _backBuffer[d + 2] * inv + 127) / 255);
                    _backBuffer[d + 3] = 255;
                }
            }
        }

        private void PixelDraw(IImage image, Rectangle area)
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
                if (k + 4 > _backBuffer.Length || k < 0)
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
            if (points == null || points.Length < 2)
                return;

            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawLine(colour, points[i], points[i + 1]);
            }
        }
        
        private void DrawLine(Colour colour, Vector2 from, Vector2 to)
        {
            int x0 = (int)Math.Round(from.X);
            int y0 = (int)Math.Round(from.Y);
            int x1 = (int)Math.Round(to.X);
            int y1 = (int)Math.Round(to.Y);

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            while (true)
            {
                if ((uint)x0 < (uint)Width && (uint)y0 < (uint)Height)
                {
                    SetPixel(x0, y0, colour);
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = err << 1;

                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void DrawRectangle(Colour colour, Rectangle area)
        {
            if (area.Width <= 0 || area.Height <= 0)
                return;

            int x0 = area.X;
            int y0 = area.Y;
            int x1 = area.X + area.Width - 1;
            int y1 = area.Y + area.Height - 1;

            // quick reject if completely offscreen
            if (x1 < 0 || y1 < 0 || x0 >= Width || y0 >= Height)
                return;

            
            DrawLine(colour, new Vector2(x0, y0), new Vector2(x1, y0)); // top
            DrawLine(colour, new Vector2(x0, y1), new Vector2(x1, y1)); // bottom
            DrawLine(colour, new Vector2(x0, y0), new Vector2(x0, y1)); // left
            DrawLine(colour, new Vector2(x1, y0), new Vector2(x1, y1)); // right
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
