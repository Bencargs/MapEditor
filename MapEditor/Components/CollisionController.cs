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
            if (collider is BoundingCircle circle)
                return this.Contains(circle);

            if (collider is BoundingBox box)
                return this.Contains(box);

            return false;
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
            if (collider is BoundingCircle circle)
                return this.Contains(circle);

            if (collider is BoundingBox box)
                return this.Contains(box);

            return false;
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
                if (t.Colliders.Any())
                {
                    return t.Colliders.FirstOrDefault();
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
