namespace MapEditor.Common
{
    // todo: temporary - replace when using a real framework
    public class Vector2
    {
        public int X;
        public int Y;

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return null;
        }

        public static Vector2 operator /(Vector2 a, float source)
        {
            return null;
        }

        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return null;
        }

        public static Vector2 operator +(Vector2 a, float b)
        {
            return null;
        }

        public static Vector2 operator *(Vector2 a, float b)
        {
            return null;
        }

        public Vector2 Truncate(float value)
        {
            return null;
        }

        public float Distance(Vector2 a)
        {
            return -1;
        }

        public float Length()
        {
            return -1;
        }

        public float Angle()
        {
            return -1;
        }

        public Vector2 Normalize()
        {
            return null;
        }
    }
}
