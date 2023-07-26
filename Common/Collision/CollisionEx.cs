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

        public static bool Contains(this BoundingPolygon a, BoundingBox b)
        {
            // Convert the rectangle into four line segments
            Vector2[] rectLines = 
            {
                new Vector2 { X = b.Location.X, Y = b.Location.Y },
                new Vector2 { X = b.Location.X + b.Width, Y = b.Location.Y },
                new Vector2 { X = b.Location.X + b.Width, Y = b.Location.Y + b.Height },
                new Vector2 { X = b.Location.X, Y = b.Location.Y + b.Height }
            };

            // Check for intersection with each line segment of the shape
            var shape = a.Points;
            for (int i = 0; i < shape.Count; i++)
            {
                int j = (i + 1) % shape.Count;
                Vector2 shapeLineStart = shape[i];
                Vector2 shapeLineEnd = shape[j];

                // Check if the rectangle and shape line segment are separated along any axis
                if (IsSeparated(rectLines, shapeLineStart, shapeLineEnd))
                {
                    return false; // Separated, so no intersection
                }
            }

            return true; // No separation along any axis, so there is an intersection
        }

        public static bool Contains(this BoundingPolygon a, BoundingCircle b)
        {
            // Check for intersection with each line segment of the shape
            var shape = a.Points;
            for (int i = 0; i < shape.Count; i++)
            {
                int j = (i + 1) % shape.Count;
                var shapeLineStart = shape[i];
                var shapeLineEnd = shape[j];

                // Check if the circle's center and the shape line segment are separated along any axis
                if (IsSeparated(b, shapeLineStart, shapeLineEnd))
                {
                    return false; // Separated, so no intersection
                }
            }

            return true; // No separation along any axis, so there is an intersection

        }

        private static bool IsSeparated(Vector2[] rectLines, Vector2 shapeLineStart, Vector2 shapeLineEnd)
        {
            // Calculate the normal vector perpendicular to the shape line segment
            var normalX = shapeLineEnd.Y - shapeLineStart.Y;
            var normalY = shapeLineStart.X - shapeLineEnd.X;

            // Normalize the normal vector
            var length = (float) Math.Sqrt(normalX * normalX + normalY * normalY);
            normalX /= length;
            normalY /= length;

            // Project each point of the rectangle and shape line segment onto the normal vector
            foreach (var point in rectLines)
            {
                var projection = (point.X - shapeLineStart.X) * normalX + (point.Y - shapeLineStart.Y) * normalY;

                // Check if the projections overlap
                if (projection < 0 || projection > length)
                {
                    return true; // Separated, so no intersection
                }
            }

            return false; // Not separated, so there is an intersection
        }

        private static bool IsSeparated(BoundingCircle circle, Vector2 shapeLineStart, Vector2 shapeLineEnd)
        {
            // Calculate the vector from the shape line segment's start to end points
            var lineVectorX = shapeLineEnd.X - shapeLineStart.X;
            var lineVectorY = shapeLineEnd.Y - shapeLineStart.Y;

            // Calculate the vector from the shape line segment's start point to the circle's center
            var startToCircleX = circle.Location.X - shapeLineStart.X;
            var startToCircleY = circle.Location.Y - shapeLineStart.Y;

            // Calculate the dot product between the line vector and the start-to-circle vector
            var dotProduct = lineVectorX * startToCircleX + lineVectorY * startToCircleY;

            // Calculate the square length of the line vector
            var lineVectorLengthSquared = lineVectorX * lineVectorX + lineVectorY * lineVectorY;

            // Project the circle's center onto the line segment
            var projection = dotProduct / lineVectorLengthSquared;

            // Calculate the closest point on the line segment to the circle's center
            float closestX, closestY;
            if (projection < 0)
            {
                closestX = shapeLineStart.X;
                closestY = shapeLineStart.Y;
            }
            else if (projection > 1)
            {
                closestX = shapeLineEnd.X;
                closestY = shapeLineEnd.Y;
            }
            else
            {
                closestX = shapeLineStart.X + projection * lineVectorX;
                closestY = shapeLineStart.Y + projection * lineVectorY;
            }

            // Calculate the distance between the closest point and the circle's center
            var distanceSquared = Vector2.DistanceSquared(circle.Location, new Vector2(closestX, closestY));

            // Check if the distance is less than the circle's radius
            if (distanceSquared <= circle.Radius * circle.Radius)
            {
                return false; // Not separated, so there is an intersection
            }

            return true; // Separated, so no intersection
        }
    }
}
