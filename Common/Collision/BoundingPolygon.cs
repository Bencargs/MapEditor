using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Common.Collision
{
    public class BoundingPolygon : ICollider
    {
        public Vector2 Location { get; set; }
        public List<Vector2> Points { get; set; }

        public bool HasCollided(ICollider collider)
        {
            if (collider is BoundingPolygon)
                throw new NotImplementedException(); // todo: seperate axis theorem for polygons

            if (collider is BoundingCircle circle)
                return this.Contains(circle);

            if (collider is BoundingBox box)
                return this.Contains(box);

            return false;
        }

        public bool Contains(Vector2 point)
        {
            bool isInside = false;
            int i, j = Points.Count- 1;
            for (i = 0; i < Points.Count; i++)
            {
                if ((Points[i].Y < point.Y && Points[j].Y >= point.Y
                     || Points[j].Y < point.Y && Points[i].Y >= point.Y)
                    && (Points[i].X <= point.X || Points[j].X <= point.X))
                {
                    if (Points[i].X + (point.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) * (Points[j].X - Points[i].X) < point.X)
                    {
                        isInside = !isInside;
                    }
                }
                j = i;
            }

            return isInside;
        }

        public ICollider Clone()
        {
            return new BoundingPolygon
            {
                Location = new Vector2(Location.X, Location.Y),
                Points = Points.ToList()
            };
        }
    }
}
