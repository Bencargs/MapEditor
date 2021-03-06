﻿using System;
using MapEditor.Handlers.CollisionHandler;

namespace MapEditor.Common
{
    public static class CollisionEx
    {
        public static bool Contains(this BoundingBox a, BoundingBox b)
        {
            return a.Position.X <= b.Position.X + b.Width &&
                   b.Position.X <= a.Position.X + a.Width &&
                   a.Position.Y <= b.Position.Y + b.Height &&
                   b.Position.Y <= a.Position.Y + a.Height;
        }

        public static bool Contains(this BoundingCircle a, BoundingCircle b)
        {
            var distanceX = a.Position.X - b.Position.X;
            var distanceY = a.Position.Y - b.Position.Y;

            var magnitudeSquared = distanceX * distanceX + distanceY * distanceY;
            return magnitudeSquared < (a.Radius + b.Radius) * (a.Radius + b.Radius);
        }

        public static bool Contains(this BoundingBox a, BoundingCircle b)
        {
            var deltaX = b.Position.X - Math.Max(a.Position.X, Math.Min(b.Position.X, a.Position.X + a.Width));
            var deltaY = b.Position.Y - Math.Max(a.Position.Y, Math.Min(b.Position.Y, a.Position.Y + a.Height));
            return (deltaX * deltaX + deltaY * deltaY) < (b.Radius * b.Radius);
        }

        public static bool Contains(this BoundingCircle a, BoundingBox b)
        {
            return b.Contains(a);
        }
    }
}
