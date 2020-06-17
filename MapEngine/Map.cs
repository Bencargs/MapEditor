using Common;

namespace MapEngine
{
    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Tile[,] Tiles { get; set; }
    }
}
