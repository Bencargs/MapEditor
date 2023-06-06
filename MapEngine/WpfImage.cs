using Common;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapEngine
{
    public class WpfImage : IImage
    {
        public int Width { get; }
        public int Height { get; }
        public WriteableBitmap Bitmap { get; set; }
        public byte[] Buffer { get; set; }

        public WpfImage(WriteableBitmap bitmap)
        {
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;
            Bitmap = bitmap;
            Buffer = Bitmap.ToByteArray();
        }

        public WpfImage(int width, int height)
            : this(new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgra32,
                null))
        {
        }

        public void Draw(byte[] buffer)
        {
            Bitmap.FromByteArray(buffer);
        }

        public IImage Rotate(float angle)
        {
            var rotated = Bitmap.RotateFree(180 - angle, false);
            if (rotated.Format != PixelFormats.Bgra32)
            {
                // Todo: for some reason rotation changes pixel format to Pbgra32 (?!)
                var bitmapSource = new FormatConvertedBitmap(rotated, PixelFormats.Bgra32, null, 0);
                rotated = new WriteableBitmap(bitmapSource);
            }

            return new WpfImage(rotated);
        }

        public IImage Scale(float scale)
        {
            var width = (int)(scale * Width);
            var height = (int)(scale * Height);
            var scaled = Bitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor); // Nearest neighbour?! BiLinear? BiCubic?
            return new WpfImage(scaled);
        }

        public IImage Fade(float fade)
        {
            for (int i = 3; i < Buffer.Length; i += 4)
            {
                Buffer[i] = (byte)Math.Max(0, Buffer[i] - fade);
            }

            return this;
        }

        public Colour this[int x, int y] 
        {
            get
            {
                var index = (x * 4) + ((y * 4) * Width);

                var colour = new Colour(
                    Buffer[index],
                    Buffer[index + 1],
                    Buffer[index + 2],
                    Buffer[index + 3]);
                return colour;
            }
            set
            {
                var index = (x * 4) + ((y * 4) * Width);
                Buffer[index] = value.Red;
                Buffer[index + 1] = value.Blue;
                Buffer[index + 2] = value.Green;
                Buffer[index + 3] = value.Alpha;
            }
        }
    }
}
