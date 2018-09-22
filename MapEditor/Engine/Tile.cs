using System;
using System.Collections.Generic;
using System.Linq;
using MapEditor.Commands;
using MapEditor.Components;
using MapEditor.Controllers.CollisionHandler;
using MapEditor.Entities;

namespace MapEditor.Engine
{
    public class Tile
    {
        public int X { get; }
        public int Y { get; }
        public bool IsDirty { get; set; }
        public Guid TerrainIndex { get; set; }
        public List<Entity> Entities { get; set; }

        public Tile(int x, int y, Guid terrainIndex)
        {
            X = x;
            Y = y;
            TerrainIndex = terrainIndex;
            IsDirty = true;
            Entities = new List<Entity>();
        }

        public IEnumerable<Entity> GetUnits()
        {
            return Entities.Where(x => x.GetComponent<UnitComponent>() != null);
        }

        public IEnumerable<ICollider> GetColliders()
        {
            //todo: use a cache?
            return Entities.Select(x => x.GetComponent<CollisionComponent>())
                .Where(x => x != null)
                .Select(x => x.Collider);
        }

        //public void Render(IGraphics graphics)
        //{
        //    if (Terrain.Image != null)
        //    {
        //        var area = new Rectangle(X, Y, Terrain.Width, Terrain.Height);
        //        graphics.DrawImage(Terrain.Image, area);
        //    }
        //}
    }
}
