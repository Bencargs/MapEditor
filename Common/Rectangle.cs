using Common.Collision;

namespace Common
{
    public class Rectangle : BoundingBox
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
