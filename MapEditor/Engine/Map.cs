using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MapEditor.Common;
using MapEditor.Engine;

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
        public bool ShowTerrain { get; set; }
        public Tile[,] Tiles { get; set; }
        public Dictionary<Guid, Terrain> Terrains { get; set; } = new Dictionary<Guid, Terrain>();
    }

    public class Map : IHandleCommand, IDisposable
    {
        private readonly MessageHub _messageHub;
        private readonly IGraphics _graphics;
        private const int CellSize = 20;
        protected MapSettings Settings { get; } = new MapSettings();

        public Map(MessageHub messageHub, IGraphics graphics, int width, int height)
        {
            Settings.Width = width;
            Settings.Height = height;

            _messageHub = messageHub;
            _graphics = graphics;
            Settings.Tiles = new Tile[width, height];
        }

        public Map(MessageHub messageHub, IGraphics graphics, MapSettings settings)
            : this(messageHub, graphics, settings.Width, settings.Height)
        {
            Settings.Tiles = settings.Tiles;
            Settings.ShowGrid = settings.ShowGrid;
            Settings.Terrains = settings.Terrains;
        }

        public void Init()
        {
            // todo: remove the requirement to manually subscribe with IHandleCommand<...> ?
            _messageHub.Subscribe(this, CommandType.PlaceTile);

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

        // todo: replace bresenhams with supercover algorithm
        public IEnumerable<Tile> GetTiles(Vector2 position)
        {
            // https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
            var startPoint = new Point(position.X, position.Y);
            var endPoint = startPoint.Add(position.Normalize() * position.Length());

            var w = endPoint.X - startPoint.X;
            var h = endPoint.Y - startPoint.Y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            var x = 0;
            var y = 0;
            var numerator = longest >> 1;
            for (var i = 0; i <= longest; i++)
            {
                yield return GetTile(new Point(x, y));

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        //// todo: compare against http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
        //// https://www.codeproject.com/Articles/30686/Bresenham-s-Line-Algorithm-Revisited
        //var startPoint = new Point(position.X, position.Y);
        //var endPoint = startPoint.Add(position.Normalize() * position.Length());

        //var deltaX = endPoint.X - startPoint.X;
        //var deltaY = endPoint.Y - endPoint.Y;
        //var error = deltaX / 2;
        //var yStep = 1;

        //if (endPoint.Y < startPoint.Y)
        //    yStep = 1;

        //var nextPoint = startPoint;
        //while (nextPoint.X < endPoint.X)
        //{
        //    if (nextPoint != startPoint)
        //        yield return GetTile(nextPoint);

        //    nextPoint.X++;
        //    error -= deltaY;
        //    if (error < 0)
        //    {
        //        nextPoint.Y += yStep;
        //        error += deltaX;
        //    }
        //}

        // https://www.redblobgames.com/grids/line-drawing.html
        //function supercover_line(p0, p1)
        //{
        //    var dx = p1.x - p0.x, dy = p1.y - p0.y;
        //    var nx = Math.abs(dx), ny = Math.abs(dy);
        //    var sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

        //    var p = new Point(p0.x, p0.y);
        //    var points = [new Point(p.x, p.y)];
        //    for (var ix = 0, iy = 0; ix < nx || iy < ny;)
        //    {
        //        if ((0.5 + ix) / nx == (0.5 + iy) / ny)
        //        {
        //            // next step is diagonal
        //            p.x += sign_x;
        //            p.y += sign_y;
        //            ix++;
        //            iy++;
        //        }
        //        else if ((0.5 + ix) / nx < (0.5 + iy) / ny)
        //        {
        //            // next step is horizontal
        //            p.x += sign_x;
        //            ix++;
        //        }
        //        else
        //        {
        //            // next step is vertical
        //            p.y += sign_y;
        //            iy++;
        //        }
        //        points.push(new Point(p.x, p.y));
        //    }
        //    return points;
        //}

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
                    else
                    {
                        Settings.Terrains[terrain.Key] = terrain;
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

        public List<Tile> GetTiles(Point point, Terrain terrain)
        {
            var lengthX = (int) Math.Ceiling((double) terrain.Width / CellSize);
            var lengthY = (int) Math.Ceiling((double) terrain.Height / CellSize);

            return (from x in Enumerable.Range(point.X, lengthX).Select(MapXToTileX)
                    from y in Enumerable.Range(point.Y, lengthY).Select(MapYToTileY)
                    select Settings.Tiles[x, y]).ToList();
        }

        public void Update()
        {
            //todo:
        }

        public void Render()
        {
            //todo: Camera.Contains - render only objects in camera view

            RenderTiles();

            if (Settings.ShowGrid)
            {
                RenderGrid();
            }

            if (Settings.ShowTerrain)
            {
                RenderTerrain();
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

        private void RenderTerrain()
        {
            Enumerate(tile =>
            {
                var terrain = Settings.Terrains[tile.TerrainIndex];

                var area = new Rectangle(tile.X, tile.Y, terrain.Width, terrain.Height);
                switch (terrain.TerrainType)
                {
                    case TerrainType.Empty:
                        break;
                    case TerrainType.Water:
                        using (var brush = new SolidBrush(Color.FromArgb(128, 0, 0, 255)))
                        {
                            _graphics.FillRectangle(brush, area);
                        }
                        break;
                    case TerrainType.Land:
                        using (var brush = new SolidBrush(Color.FromArgb(128, 0, 128, 0)))
                        {
                            _graphics.FillRectangle(brush, area);
                        }
                        break;
                    case TerrainType.ImpassableLand:
                        using (var brush = new SolidBrush(Color.FromArgb(128, 128, 128, 128)))
                        {
                            _graphics.FillRectangle(brush, area);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
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

        public void Handle(ICommand command)
        {
            // todo: implement IHandleCommand<T> to remove this switch statement
            switch (command)
            {
                case PlaceTileCommand c:
                    SetTile(c.Point, c.Terrain);
                    break;
            }
        }

        public void Undo(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
