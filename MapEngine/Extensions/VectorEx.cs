using System;
using System.Numerics;

namespace MapEngine
{
    public static class VectorEx
    {
        public static float Angle(this Vector2 source)
        {
            return (float)(Math.Atan2(source.Y, source.X) * 360.0 / (2 * Math.PI));
        }

        /// <summary>
        /// A
        /// | \
        /// |   \
        /// |___( B
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float Angle(this Vector2 source, Vector2 target)
        {
            double sin = source.X * target.Y - target.X * source.Y;
            double cos = source.X * target.X + source.Y * target.Y;

            return (float)(Math.Atan2(sin, cos) * (180 / Math.PI));
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

        public static Vector3 Truncate(this Vector3 source, float max)
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

        public static Vector3 Normalize(this Vector3 source)
        {
            return Vector3.Normalize(source);
        }

        public static Vector2 ToVector2(this Vector3 source)
        {
            return new Vector2(source.X, source.Y);
        }
    }
}
