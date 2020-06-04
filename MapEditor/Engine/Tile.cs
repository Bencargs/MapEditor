using System;

namespace MapEditor.Engine
{
    public class Tile
    {
        public Guid Id { get; set; }
        public int X { get; }
        public int Y { get; }
        public Guid TerrainIndex { get; set; }

        public Tile(int x, int y, Guid terrainIndex)
        {
            X = x;
            Y = y;
            TerrainIndex = terrainIndex;
            Id = GuidComb.ToGuid($"{x}{y}{TerrainIndex}");
        }
    }
}
