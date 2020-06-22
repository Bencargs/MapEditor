using System;
using System.Numerics;

namespace Common.Collision
{
    internal static class CollisionEx
    {
        public static bool Contains(this BoundingBox a, BoundingBox b)
        {
            return a.Location.X <= b.Location.X + b.Width &&
                   b.Location.X <= a.Location.X + a.Width &&
                   a.Location.Y <= b.Location.Y + b.Height &&
                   b.Location.Y <= a.Location.Y + a.Height;
        }

        public static bool Contains(this BoundingCircle a, BoundingCircle b)
        {
            var distanceX = a.Location.X - b.Location.X;
            var distanceY = a.Location.Y - b.Location.Y;

            var magnitudeSquared = distanceX * distanceX + distanceY * distanceY;
            return magnitudeSquared < (a.Radius + b.Radius) * (a.Radius + b.Radius);
        }

        public static bool Contains(this BoundingBox a, BoundingCircle b)
        {
            var deltaX = b.Location.X - Math.Max(a.Location.X, Math.Min(b.Location.X, a.Location.X + a.Width));
            var deltaY = b.Location.Y - Math.Max(a.Location.Y, Math.Min(b.Location.Y, a.Location.Y + a.Height));
            return (deltaX * deltaX + deltaY * deltaY) < (b.Radius * b.Radius);
        }

        public static bool Contains(this BoundingCircle a, BoundingBox b)
        {
            return b.HasCollided(a);
        }
    }
}
