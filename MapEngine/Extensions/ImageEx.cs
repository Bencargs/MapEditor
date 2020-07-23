using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
    }
}
