using Common;
using MapEngine.Services.Effects.FluidEffect;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MapEngine.Services.Map
{
    public class MapService
    {
        private Map _map;

        public Tile[,] Tiles => _map.Tiles;
        public int Width => _map.Width;
        public int Height => _map.Height;
        public FluidEffects FluidEffects => _map.FluidEffects; // todo: this all seems a little redundant, whats the value add here?

        public void Initialise(Map map)
        {
            _map = map;
        }

        public Tile GetTile(Vector2 location)
        {
            var x = (int)((location.X / Width) * _map.Tiles.GetLength(0));
            var y = (int)((location.Y / Height) * _map.Tiles.GetLength(1));
            x = Math.Max(0, Math.Min(x, _map.Tiles.GetLength(0) - 1));
            y = Math.Max(0, Math.Min(y, _map.Tiles.GetLength(1) - 1));
            return _map.Tiles[x, y];
        }

        public (int X, int Y) GetCoordinates(Tile tile)
        {
            for (int x = 0; x < _map.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < _map.Tiles.GetLength(1); y++)
                {
                    if (_map.Tiles[x, y].Id == tile.Id)
                        return (x, y);
                }
            }
            return (0, 0);
        }
    }
}
