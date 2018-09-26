using System;
using System.Drawing;
using System.Security.Cryptography;

namespace MapEditor.Common
{
    public static class Extensions
    {
        public static Point Add(this Point b, Vector2 a)
        {
            return new Point(0, 0);
        }

        public static Point Minus(this Point a, Point b)
        {
            return new Point(0, 0);
        }

        public static float Distance(this Point a, Point b)
        {
            return (float) Math.Sqrt(((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)));
        }

        public static byte[] GetImageHashcode(this Image image)
        {
            var converter = new ImageConverter();
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var rawImageData = converter.ConvertTo(image, typeof(byte[])) as byte[];
                return rawImageData != null ? md5.ComputeHash(rawImageData) : null;
            }
        }
    }
}
