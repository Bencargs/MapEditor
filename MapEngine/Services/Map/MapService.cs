using Common;
using MapEngine.Factories;
using MapEngine.Services.Effects.FluidEffect;
using MapEngine.Services.Effects.WaveEffect;
using System;
using Vector2 = System.Numerics.Vector2;

namespace MapEngine.Services.Map
{
    public class MapService
    {
        private Map _map;

        // todo: kill 'textureTiles' with fire once figured out why rendering is so slow
        public Tile[,] TextureTiles => _map.Tiles;
        public Tile[,] PathfindingTiles;
        public int Scale { get; private set; }
        public int Width => _map.Width;
        public int Height => _map.Height;
        public FluidEffects FluidEffects => _map.FluidEffects; // todo: this all seems a little redundant, whats the value add here?
        public WaveEffects WaveEffects => _map.WaveEffects;

        public void Initialise(Map map)
        {
            _map = map;
            Scale = 4;
            ResizeTileArray(_map, 4);
        }

        public Tile GetTile(Vector2 location)
        {
            var gridWidth = PathfindingTiles.GetLength(0);
            var gridHeight = PathfindingTiles.GetLength(1);
            var x = (int)((location.X / Width) * gridWidth);
            var y = (int)((location.Y / Height) * gridHeight);
            x = Math.Max(0, Math.Min(x, gridWidth - 1));
            y = Math.Max(0, Math.Min(y, gridHeight - 1));
            return PathfindingTiles[x, y];
        }

        public int GetHeight(Vector2 location)
        {
            var tile = GetTile(location);

            if (TextureFactory.TryGetTexture(tile.HeightmapTextureId, out var heightmap))
            {
                // todo: use full RGB colour range to represent heights for better verticality
                // see https://jonathancritchley.ca/TOHeight_post.html
                if (location.X < 0 || location.X > heightmap.Image.Width - 1 ||
                    location.Y < 0 || location.Y > heightmap.Image.Height - 1)
                    return 0;

                return heightmap.Image[(int)location.X, (int)location.Y].Blue;
            }

            return 0;
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

        // We want an array of 4x4 tiles, rather than the large texture tiles in the map
        private void ResizeTileArray(Map map, int scale)
        {
            var originalWidth = map.Tiles.GetLength(0);
            var originalHeight = map.Tiles.GetLength(1);
            var newWidth = map.Width / scale;
            var newHeight = map.Height / scale;
            var tileWidth = (float)originalWidth / newWidth;
            var tileHeight = (float)originalHeight / newHeight;

            PathfindingTiles = new Tile[newWidth, newHeight];

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    // Calculate the indices for the original tile
                    int originalX = (int)(x * tileWidth);
                    int originalY = (int)(y * tileHeight);
                    var originalTile = map.Tiles[originalX, originalY];

                    // Create a new tile and copy properties from the original tile
                    PathfindingTiles[x, y] = new Tile
                    {
                        Id = y * newWidth + x,
                        Location = new Vector2(x * scale, y * scale),
                        TextureId = originalTile.TextureId,
                        Type = originalTile.Type,
                        HeightmapTextureId = originalTile.HeightmapTextureId
                    };
                }
            }
        }
    }
}
