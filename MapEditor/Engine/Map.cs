using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace MapEditor
{
    public class MapSettings
    {
        [JsonConverter(typeof(JsonImageConverter))]
        public Image Background { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ShowGrid { get; set; }
        public Tile[,] Tiles { get; set; }
    }

    public class Map : IDisposable
    {
        private readonly Tile[,] _tiles;
        private readonly IGraphics _graphics;
        private const int CellSize = 20;
        public Image Background { get; set; }
        public int Width { get; }
        public int Height { get; }
        public bool ShowGrid { get; set; }
        // todo: incorporate into MapData
        public Dictionary<Guid, Terrain> Terrains { get; set; } = new Dictionary<Guid, Terrain>();

        public Map(IGraphics graphics, int width, int height)
        {
            Width = width;
            Height = height;

            _graphics = graphics;
            _tiles = new Tile[Width, Height];
        }

        public Map(IGraphics graphics, MapSettings settings)
            : this(graphics, settings.Width, settings.Height)
        {
            Background = settings.Background;
            ShowGrid = settings.ShowGrid;

            _tiles = settings.Tiles;
            _graphics = graphics;
        }

        public void Init()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var worldX = x * CellSize;
                    var worldY = y * CellSize;
                    var terrain = new Terrain(TerrainType.Empty, null, Width, Height);
                    if (!Terrains.ContainsKey(terrain.Key))
                    {
                        Terrains.Add(terrain.Key, terrain);
                    }

                    _tiles[x, y] = new Tile(worldX, worldY, terrain.Key);
                }
            }
        }

        public MapSettings Save()
        {
            return new MapSettings
            {
                Background = Background,
                Width = Width,
                Height = Height,
                ShowGrid = ShowGrid,
                Tiles = _tiles
            };
        }

        private int MapXToTileX(int x)
        {
            var tileX = x * Width / _graphics.Width;
            
            // todo: Should be uneccessary as long as all code correctly calls Enumerate
            if (tileX == 0)
                tileX = 1;
            else if (tileX >= Width - 1)
                tileX = Width - 2;

            return tileX;
        }

        private int MapYToTileY(int y)
        {
            var tileY = y * Height / _graphics.Height;

            // todo: Should be uneccessary as long as all code correctly calls Enumerate
            if (tileY == 0)
                tileY = 1;
            else if (tileY >= Height - 1)
                tileY = Height - 2;

            return tileY;
        }

        public Tile GetTile(Point point)
        {
            var x = MapXToTileX(point.X);
            var y = MapYToTileY(point.Y);
            
            return _tiles[x, y];
        }

        public void SetTile(Point point, Terrain tile)
        {
            var x = MapXToTileX(point.X);
            var y = MapYToTileY(point.Y);

            var image = new Bitmap(tile.Image);
            for (var i = 0; i < tile.Image.Width / tile.Width; i++)
            {
                for (var j = 0; j < tile.Image.Height / tile.Height; j++)
                {
                    var area = new Rectangle(i * tile.Width, j * tile.Height, tile.Width, tile.Height);
                    var cropped = image.Clone(area, image.PixelFormat);

                    var terrain = new Terrain(tile.TerrainType, cropped, tile.Width, tile.Height);
                    if (!Terrains.ContainsKey(terrain.Key))
                    {
                        Terrains.Add(terrain.Key, terrain);
                    }

                    var offsetX = x + i > Width - 1 ? Width - 1 : x + i;
                    var offsetY = y + j > Height - 1 ? Height - 1 : y + j;
                    var previous = _tiles[offsetX, offsetY];
                    _tiles[offsetX, offsetY] = new Tile(previous.X, previous.Y, terrain.Key);
                }
            }
        }

        public Terrain GetTerrain(Guid terrainKey)
        {
            Terrains.TryGetValue(terrainKey, out Terrain terrain);
            return terrain;
        }

        public void Update()
        {
            //todo:
        }

        public void Render()
        {
            RenderBackground();

            RenderTiles();

            if (ShowGrid)
            {
                RenderGrid();
            }
        }

        private void RenderBackground()
        {
            if (Background != null)
            {
                _graphics.DrawImage(Background, new Rectangle(0, 0, Background.Width, Background.Height));
            }
        }

        private void RenderTiles()
        {
            Enumerate(tile =>
            {
                // todo: replace with foreach (var tile in DirtyTiles)
                //if (!tile.IsDirty)
                //    continue;

                //tile.Render(_graphics);
                if (Terrains.TryGetValue(tile.TerrainIndex, out Terrain terrain) && terrain.Image != null)
                {
                    var area = new Rectangle(tile.X, tile.Y, terrain.Width, terrain.Height);
                    _graphics.DrawImage(terrain.Image, area);
                }
                //tile.IsDirty = false;
            });
        }

        private void Enumerate(Action<Tile> action)
        {
            // todo: Ensure camera cannot see map indexs 0
            // Starting at index 1 prevents ever having to do bounds checks
            for (var x = 1; x < Width - 1; x++)
            {
                for (var y = 1; y < Height - 1; y++)
                {
                    action(_tiles[x, y]);
                }
            }
        }

        private void RenderGrid()
        {
            // todo: just save this to an image?
            for (var x = 0; x < Width; x++)
            {
                var points = new[]
                {
                    new Point(x * CellSize, 0),
                    new Point(x * CellSize, _graphics.Height)
                };
                _graphics.DrawLines(Color.LightBlue, points);
            }

            for (var y = 0; y< Height; y++)
            {
                var points = new[]
                {
                    new Point(0, y * CellSize),
                    new Point(_graphics.Width, y * CellSize)
                };
                _graphics.DrawLines(Color.LightBlue, points);
            }
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            Background?.Dispose();
            if (Terrains != null)
            {
                foreach (var t in Terrains.Values)
                {
                    t.Dispose();
                }
            }
        }
    }
}
