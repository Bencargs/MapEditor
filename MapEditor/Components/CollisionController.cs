using System;
using System.Drawing;
using System.Linq;
using MapEditor.Common;

namespace MapEditor.Components
{
    public interface ICollider
    {
        Point Position { get; set; }
        bool IsCollided(ICollider collider);
    }

    public class BoundingCircle : ICollider
    {
        public float Radius { get; set; }
        public Point Position { get; set; }

        public bool IsCollided(ICollider collider)
        {
            var circle = collider as BoundingCircle;
            if (circle != null)
            {
                return IsCollided(circle);
            }

            var box = collider as BoundingBox;
            if (box != null)
            {
                return IsCollided(box);
            }
            return false;
        }

        private bool IsCollided(BoundingCircle collider)
        {
            var distanceX = Position.X - collider.Position.X;
            var distanceY = Position.Y - collider.Position.Y;

            var magnitudeSquared = distanceX * distanceX + distanceY * distanceY;
            return magnitudeSquared < (Radius + collider.Radius) * (Radius + collider.Radius);
        }

        private bool IsCollided(BoundingBox box)
        {
            var deltaX = Position.X - Math.Max(box.Position.X, Math.Min(Position.X, box.Position.X + box.Width));
            var deltaY = Position.Y - Math.Max(box.Position.Y, Math.Min(Position.Y, box.Position.Y + box.Height));
            return (deltaX * deltaX + deltaY * deltaY) < (Radius * Radius);
        }
    }

    public class BoundingBox : ICollider
    {
        //todo: Non-axis alligned: https://yal.cc/rot-rect-vs-circle-intersection/

        public float Width { get; set; }
        public float Height { get; set; }
        public Point Position { get; set; }

        public bool IsCollided(ICollider collider)
        {
            var circle = collider as BoundingCircle;
            if (circle != null)
            {
                return IsCollided(circle);
            }

            var box = collider as BoundingBox;
            if (box != null)
            {
                return IsCollided(box);
            }
            return false;
        }

        private bool IsCollided(BoundingCircle circle)
        {
            var deltaX = circle.Position.X - Math.Max(Position.X, Math.Min(circle.Position.X, Position.X + Width));
            var deltaY = circle.Position.Y - Math.Max(Position.Y, Math.Min(circle.Position.Y, Position.Y + Height));
            return (deltaX * deltaX + deltaY * deltaY) < (circle.Radius * circle.Radius);
        }

        private bool IsCollided(BoundingBox box)
        {
            return (Position.X <= box.Position.X + box.Width &&
                    box.Position.X <= Position.X + Width &&
                    Position.Y <= box.Position.Y + box.Height &&
                    box.Position.Y <= Position.Y + Height);
        }
    }

    public class CollisionController
    {
        private readonly Map _map;

        public CollisionController(Map map)
        {
            _map = map;
        }

        public ICollider GetCollision(Vector2 line)
        {
            //ensure tiles contain colliders?
            // foreach cell from vector start to vector end
            // get collider - check collission - if true return, else keep going
            // return null
            var tiles = _map.GetTiles(line);
            foreach (var t in tiles)
            {
                //line circle intersection - https://yal.cc/gamemaker-collision-line-point/
                //https://stackoverflow.com/questions/23016676/line-segment-and-circle-intersection
                //return t.Colliders.FirstOrDefault();
                foreach (var c in t.Colliders)
                {

                }
            }
            return null;
        }

        public ICollider GetCollision(ICollider circle)
        {
            float oldDistance = 0;
            ICollider collider = null;


            var tile = _map.GetTile(circle.Position);
            foreach (var c in tile.Colliders)
            {
                if (!c.IsCollided(circle))
                    continue;

                if (collider != null)
                {
                    var newDistance = circle.Position.Distance(c.Position);
                    if (newDistance < oldDistance)
                        collider = c;
                }
                else
                {
                    oldDistance = circle.Position.Distance(c.Position);
                    collider = c;
                }
            }
            return collider;
        }
    }
}
