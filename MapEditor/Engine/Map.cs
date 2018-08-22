using System;
using System.Collections.Generic;
using System.Drawing;

namespace MapEditor
{
    public class MapSettings
    {
        // Tiles & Terrains should contain all Background data
        //[JsonConverter(typeof(JsonImageConverter))]
        //public Image Background { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ShowGrid { get; set; }
        public Tile[,] Tiles { get; set; }
        public Dictionary<Guid, Terrain> Terrains { get; set; } = new Dictionary<Guid, Terrain>();
    }

    public class Map : IDisposable
    {
        private readonly IGraphics _graphics;
        private const int CellSize = 20;
        private MapSettings Settings { get; } = new MapSettings();

        public Map(IGraphics graphics, int width, int height)
        {
            Settings.Width = width;
            Settings.Height = height;

            _graphics = graphics;
            Settings.Tiles = new Tile[width, height];
        }

        public Map(IGraphics graphics, MapSettings settings)
            : this(graphics, settings.Width, settings.Height)
        {
            Settings.Tiles = settings.Tiles;
            Settings.ShowGrid = settings.ShowGrid;
            Settings.Terrains = settings.Terrains;
        }

        public void Init()
        {
            for (var x = 0; x < Settings.Width; x++)
            {
                for (var y = 0; y < Settings.Height; y++)
                {
                    var worldX = x * CellSize;
                    var worldY = y * CellSize;
                    var terrain = new Terrain(TerrainType.Empty, null, Settings.Width, Settings.Height);
                    if (!Settings.Terrains.ContainsKey(terrain.Key))
                    {
                        Settings.Terrains.Add(terrain.Key, terrain);
                    }

                    Settings.Tiles[x, y] = new Tile(worldX, worldY, terrain.Key);
                }
            }
        }

        public void ShowGrid(bool show)
        {
            Settings.ShowGrid = show;
        }

        public MapSettings Save()
        {
            return Settings;
        }

        private int MapXToTileX(int x)
        {
            var tileX = x * Settings.Width / _graphics.Width;
            
            // todo: Should be uneccessary as long as all code correctly calls Enumerate
            if (tileX == 0)
                tileX = 1;
            else if (tileX >= Settings.Width - 1)
                tileX = Settings.Width - 2;

            return tileX;
        }

        private int MapYToTileY(int y)
        {
            var tileY = y * Settings.Height / _graphics.Height;

            // todo: Should be uneccessary as long as all code correctly calls Enumerate
            if (tileY == 0)
                tileY = 1;
            else if (tileY >= Settings.Height - 1)
                tileY = Settings.Height - 2;

            return tileY;
        }

        public Tile GetTile(Point point)
        {
            var x = MapXToTileX(point.X);
            var y = MapYToTileY(point.Y);
            
            return Settings.Tiles[x, y];
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
                    if (!Settings.Terrains.ContainsKey(terrain.Key))
                    {
                        Settings.Terrains.Add(terrain.Key, terrain);
                    }

                    var offsetX = x + i > Settings.Width - 1 ? Settings.Width - 1 : x + i;
                    var offsetY = y + j > Settings.Height - 1 ? Settings.Height - 1 : y + j;
                    var previous = Settings.Tiles[offsetX, offsetY];
                    Settings.Tiles[offsetX, offsetY] = new Tile(previous.X, previous.Y, terrain.Key);
                }
            }
        }

        public Terrain GetTerrain(Guid terrainKey)
        {
            Settings.Terrains.TryGetValue(terrainKey, out Terrain terrain);
            return terrain;
        }

        public void Update()
        {
            //todo:
        }

        public void Render()
        {
            RenderTiles();

            if (Settings.ShowGrid)
            {
                RenderGrid();
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
                if (Settings.Terrains.TryGetValue(tile.TerrainIndex, out Terrain terrain) && terrain.Image != null)
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
            for (var x = 1; x < Settings.Width - 1; x++)
            {
                for (var y = 1; y < Settings.Height - 1; y++)
                {
                    action(Settings.Tiles[x, y]);
                }
            }
        }

        private void RenderGrid()
        {
            // todo: just save this to an image?
            for (var x = 0; x < Settings.Width; x++)
            {
                var points = new[]
                {
                    new Point(x * CellSize, 0),
                    new Point(x * CellSize, _graphics.Height)
                };
                _graphics.DrawLines(Color.LightBlue, points);
            }

            for (var y = 0; y < Settings.Height; y++)
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
            if (Settings.Terrains != null)
            {
                foreach (var t in Settings.Terrains.Values)
                {
                    t.Dispose();
                }
            }
        }
    }
}
