using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common;

namespace MapEngine
{
    public static class ImageEx
    {
        public static WriteableBitmap Scale(this WriteableBitmap image, double scale)
        {
            var s = new ScaleTransform(scale, scale);

            var source = new TransformedBitmap(image, s);

            // Calculate stride of source
            int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);

            // Create data array to hold source pixel data
            byte[] data = new byte[stride * source.PixelHeight];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            // Create WriteableBitmap to copy the pixel data to.      
            WriteableBitmap target = new WriteableBitmap(source.PixelWidth
                , source.PixelHeight, source.DpiX, source.DpiY
                , source.Format, null);

            // Write the pixel data to the WriteableBitmap.
            target.WritePixels(new Int32Rect(0, 0
                , source.PixelWidth, source.PixelHeight)
                , data, stride, 0);

            return target;
        }

        public static Rectangle Area(this IImage image, Vector2 point)
        {
            var x = point.X - (image.Width / 2);
            var y = point.Y - (image.Height / 2);
            return new Rectangle((int)x, (int)y, image.Width, image.Height);
        }

        public static Colour GetPalette(this IImage palette, int index, int range)
        {
            var width = palette.Width;
            var low = (int) Math.Min(width - 1, Math.Floor(((float)index / range) * width));
            var high = Math.Min(width - 1, low + 1);
            var lowColour = palette[low, 0];
            var highColour = palette[high, 0];

            var interpolation = (index % ((float)range / width)) / 10;

            return lowColour.Interpolate(highColour, interpolation);
        }

        public static Colour Interpolate(
            this Colour endPoint1,
            Colour endPoint2,
            double fraction)
        {
            fraction %= 1;

            var color = new Colour(
                InterpolateComponent(endPoint1, endPoint2, fraction, x => x.Red),
                InterpolateComponent(endPoint1, endPoint2, fraction, x => x.Blue),
                InterpolateComponent(endPoint1, endPoint2, fraction, x => x.Green),
                255);

            return color;
        }

        private static byte InterpolateComponent(
            Colour endPoint1,
            Colour endPoint2,
            double lambda,
            Func<Colour, byte> selector)
            => (byte)(selector(endPoint1) + (selector(endPoint2) - selector(endPoint1)) * lambda);

        public static void ChangeHue(this IImage bmp, Colour colour)
        {
            for (var x = 0; x < bmp.Width; x++)
            {
                for (var y = 0; y < bmp.Height; y++)
                {
                    var existing = bmp[x, y];

                    if (existing.Alpha != 0)
                    {
                        var redIntensity = (float)existing.Red / 256;
                        var blueIntensity = (float)existing.Blue / 256;
                        var greenIntensity = (float)existing.Green / 256;

                        var r = colour.Red * redIntensity;
                        var g = colour.Green * greenIntensity;
                        var b = colour.Blue * blueIntensity;

                        var newColour = new Colour(
                            (byte)r,
                            (byte)b,
                            (byte)g,
                            existing.Alpha);
                        bmp[x, y] = newColour;
                    }
                }
            }
        }
    }
}
