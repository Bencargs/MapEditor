using System;
using System.Collections.Generic;

namespace MapEditor.Engine
{
    public class MapSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ShowGrid { get; set; }
        public bool ShowTerrain { get; set; }
        public Tile[,] Tiles { get; set; }
        public Dictionary<Guid, Terrain> Terrains { get; set; } = new Dictionary<Guid, Terrain>();
    }
}
