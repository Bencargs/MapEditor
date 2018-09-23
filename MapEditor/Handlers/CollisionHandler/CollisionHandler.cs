using System.Linq;
using MapEditor.Common;
using MapEditor.Engine;

namespace MapEditor.Handlers.CollisionHandler
{
    public class CollisionHandler
    {
        private readonly Map _map;

        public CollisionHandler(Map map)
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

                var collider = t.GetColliders().FirstOrDefault();
                if (collider != null)
                    return collider;
            }
            return null;
        }

        public ICollider GetCollision(ICollider circle)
        {
            float oldDistance = 0;
            ICollider collider = null;

            var tile = _map.GetTile(circle.Position);
            foreach (var c in tile.GetColliders())
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
