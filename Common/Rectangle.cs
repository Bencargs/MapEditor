using System.Numerics;

namespace Common
{
    public class Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Vector2 point, int width, int height)
        {
            X = (int) point.X;
            Y = (int) point.Y;
            Width = width;
            Height = height;
        }

        public bool Contains(Vector2 point)
        {
            return X <= point.X &&
                   point.X < X + Width &&
                   Y <= point.Y &&
                   point.Y < Y + Height;
        }

        public void Translate(int x, int y)
        {
            X += x;
            Y += y;
        }
    }
}
