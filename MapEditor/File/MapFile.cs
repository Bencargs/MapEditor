using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditor.Engine;
using MapEditor.Entities;

namespace MapEditor.File
{
    public class MapFile
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ShowGrid { get; set; }
        public bool ShowTerrain { get; set; }
        public Tile[,] Tiles { get; set; }
        public Dictionary<Guid, Terrain> Terrains { get; set; } = new Dictionary<Guid, Terrain>();
        public Dictionary<Guid, List<Entity>> Units { get; set; } = new Dictionary<Guid, List<Entity>>();
        public Rectangle Viewport { get; set; }
    }
}
