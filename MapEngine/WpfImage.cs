using Common;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapEngine
{
    public class WpfImage : IImage
    {
        public int Width { get; }
        public int Height { get; }
        public WriteableBitmap Bitmap { get; set; } // todo: ToBitmap?
        public byte[] Buffer { get; set; }

        public WpfImage(WriteableBitmap bitmap)
        {
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;
            Bitmap = bitmap;

            Buffer = new byte[Width * Height * 4];
            Bitmap.CopyPixels(Buffer, Width * 4, 0);
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
            var area = new Int32Rect(0, 0, Width, Height);
            Bitmap.WritePixels(area, buffer, Width * 4, 0);
        }

        public IImage Rotate(float angle)
        {
            var rotated = Bitmap.RotateFree(180 - angle, false);
            return new WpfImage(rotated);
        }

        public IImage Scale(float scale)
        {
            var width = (int)scale * Width;
            var height = (int)scale * Height;
            var scaled = Bitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            return new WpfImage(scaled);
        }

        public Colour this[int x, int y] 
        {
            get
            {
                var index = x + (y * Width);
                var colour = new Colour(
                    Buffer[index],
                    Buffer[index + 1],
                    Buffer[index + 2],
                    Buffer[index + 3]);
                return colour;
            }
            set => throw new NotImplementedException(); 
        }
    }
}
