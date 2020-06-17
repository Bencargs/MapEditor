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
        public WriteableBitmap Bitmap { get; set; }
        private readonly byte[] _buffer;

        public WpfImage(WriteableBitmap bitmap)
        {
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;
            Bitmap = bitmap;

            _buffer = new byte[Width * Height * 4];
            Bitmap.CopyPixels(_buffer, Width * 4, 0);
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

        public Colour this[int x, int y] 
        {
            get
            {
                var index = x + (y * Width);
                var colour = new Colour(
                    _buffer[index],
                    _buffer[index + 1],
                    _buffer[index + 2],
                    _buffer[index + 3]);
                return colour;
            }
            set => throw new NotImplementedException(); 
        }
    }
}
