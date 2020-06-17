using System;
using System.Numerics;

namespace MapEngine
{
    public static class VectorEx
    {
        public static float Angle(this Vector2 source)
        {
            return (float)(Math.Atan2(source.Y, source.X) + Math.PI / 2);
        }

        public static Vector2 Truncate(this Vector2 source, float max)
        {
            if (source.Length() > max)
            {
                source = source.Normalize();
                source *= max;
            }
            return source;
        }

        public static Vector2 Normalize(this Vector2 source)
        {
            var distance =  Math.Sqrt(source.X * source.X + source.Y * source.Y);
            return new Vector2((float)(source.X / distance), (float)(source.Y / distance));
        }

        public static float Distance(this Vector2 source, Vector2 destination)
        {
            return Vector2.Distance(source, destination);
        }
    }
}
