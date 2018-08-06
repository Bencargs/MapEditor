using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor
{
    public enum Terrain
    {
        Land = 0,
        ImpassableLand,
        Water
    }

    public class Tile
    {
        public Terrain Terrain { get; set; } = Terrain.Land;
    }
}
