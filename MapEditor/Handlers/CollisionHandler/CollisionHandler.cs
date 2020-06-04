using System.Linq;
using MapEditor.Common;
using MapEditor.Components;
using MapEditor.Repository;

namespace MapEditor.Handlers.CollisionHandler
{
    public class CollisionHandler
    {
        private readonly ISession _session;

        public CollisionHandler(ISession session)
        {
            _session = session;
        }

        public ICollider GetCollision(Vector2 line)
        {
            //ensure tiles contain colliders?
            // foreach cell from vector start to vector end
            // get collider - check collission - if true return, else keep going
            // return null

            //line circle intersection - https://yal.cc/gamemaker-collision-line-point/
            //https://stackoverflow.com/questions/23016676/line-segment-and-circle-intersection
            return _session.GetComponent<CollisionComponent>(line)
                ?.FirstOrDefault()
                ?.Collider;
        }

        public ICollider GetCollision(ICollider circle)
        {
            float oldDistance = 0;
            ICollider collider = null;

            var colliders = _session.GetComponent<CollisionComponent>(circle.Position)
                .Select(x => x.Collider);
            foreach (var c in colliders)
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
