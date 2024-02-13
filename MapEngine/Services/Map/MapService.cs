using Common;
using MapEngine.Factories;
using MapEngine.Services.Effects.FluidEffect;
using MapEngine.Services.Effects.WaveEffect;
using System;
using System.Numerics;
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

        public int GetElevation(Vector2 location)
        {
            //var tile = GetTile(location);
            // todo: should each tile have a heightmap?
            // pretty cumbersome to load in thousands of differant heightmaps for a map file
            // what about a single heightmap is divided into individual tile heights?

            //options:
            // 1. get heightmap texture
            // find the relative point in the texture of this location
            // tiles are uneccessary

            // 2. get texture on load
            // divide up heightmap to each tile
            // save a byte array to each tile

            var tile = GetTile(new Vector2(0, 0));

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

        public void CalculateNormals(Tile[,] tiles)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //todo: average all points of elevation in tile bounds?
                    // Get the height values of neighboring tiles
                    int elevation = GetElevation(tiles[x, y].Location);
                    int elevationLeft = (x > 0) ? GetElevation(tiles[x - 1, y].Location) : elevation;
                    int elevationRight = (x < width - 1) ? GetElevation(tiles[x + 1, y].Location) : elevation;
                    int elevationUp = (y > 0) ? GetElevation(tiles[x, y - 1].Location) : elevation;
                    int elevationDown = (y < height - 1) ? GetElevation(tiles[x, y + 1].Location) : elevation;

                    // Calculate the slopes in the X and Y directions
                    float slopeX = elevationRight - elevationLeft;
                    float slopeY = elevationDown - elevationUp;

                    // Calculate the normal vector using the cross-product method
                    Vector3 normal = Vector3.Normalize(new Vector3(-slopeX, -slopeY, 2f));

                    tiles[x, y].Normal = normal;
                }
            }
        }

        public float GetFriction(Vector2 location)
        {
            // todo: custom friction lookup for each terrain type?
            // terrainType.Friction * surfaceGradient?

            var tile = GetTile(location);
            var surfaceGradient = tile.GetGradient();

            // todo: this logic seems backwards
            float highFrictionCoefficient = 0.2f;     // High friction coefficient for steep surfaces
            float mediumFrictionCoefficient = 0.5f;   // Medium friction coefficient for moderate surfaces
            float lowFrictionCoefficient = 0.95f;      // Low friction coefficient for gentle surfaces

            // Determine the friction value based on the surface gradient
            if (surfaceGradient > 45.0f)
            {
                return highFrictionCoefficient;
            }

            if (surfaceGradient > 15.0f)
            {
                return mediumFrictionCoefficient;
            }

            return lowFrictionCoefficient;
        }

        // We want a big array of small tiles for nimbler pathfinding
        // rather than the large texture tiles in the map
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
                        HeightmapTextureId = originalTile.HeightmapTextureId,
                        //Normal = CalculateNormal(x, y)
                    };
                }
            }
            CalculateNormals(PathfindingTiles);
        }
    }
}
