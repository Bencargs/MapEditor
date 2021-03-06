﻿using Common;

namespace MapEngine.Services.Map
{
    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Team[] Teams { get; set; }
        public Tile[,] Tiles { get; set; }
    }
}
