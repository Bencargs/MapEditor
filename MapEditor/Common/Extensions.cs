using System;
using System.Drawing;

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
    }
}
