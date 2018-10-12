using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditor.Commands;
using MapEditor.Common;
using MapEditor.Repository;

namespace MapEditor.Engine
{
    public class Map : IHandleCommand, IDisposable
    {
        private readonly MessageHub _messageHub;
        private readonly ISession _session;
        private const int CellSize = 20;
        protected MapSettings Settings { get; private set; } = new MapSettings();

        public Map(MessageHub messageHub, ISession session, int width, int height)
        {
            _messageHub = messageHub;
            _session = session;

            _messageHub.Post(new CreateMapCommand
            {
                MapSettings = new MapSettings
                {
                    Width = width,
                    Height = height
                }
            });
        }

        public Map(MessageHub messageHub, ISession session, MapSettings settings)
        {
            _session = session;
            _messageHub = messageHub;
            _messageHub.Post(new CreateMapCommand
            {
                MapSettings = settings
            });
        }

        public void Init()
        {
            Settings = _session.GetMapSettings();

            // todo: remove the requirement to manually subscribe with IHandleCommand<...> ?
            _messageHub.Subscribe(this, CommandType.PlaceTile);

            var tiles = _session.GetTiles();
            var terrains = _session.GetTerrains();
            for (var x = 0; x < Settings.Width; x++)
            {
                for (var y = 0; y < Settings.Height; y++)
                {
                    var worldX = x * CellSize;
                    var worldY = y * CellSize;
                    var terrain = new Terrain(TerrainType.Empty, null, Settings.Width, Settings.Height);
                    if (!terrains.ContainsKey(terrain.Key))
                    {
                        terrains.Add(terrain.Key, terrain);
                    }

                    tiles[x, y] = new Tile(worldX, worldY, terrain.Key);
                }
            }
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
                yield return _session.GetTile(new Point(x, y));

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
            if (tile.Image == null)
            {
                return;
            }

            var mapTile = _session.GetTile(point);

            var image = new Bitmap(tile.Image);
            for (var i = 0; i < tile.Image.Width / tile.Width; i++)
            {
                for (var j = 0; j < tile.Image.Height / tile.Height; j++)
                {
                    var area = new Rectangle(i * tile.Width, j * tile.Height, tile.Width, tile.Height);
                    var cropped = image.Clone(area, image.PixelFormat);

                    var terrains = _session.GetTerrains();
                    var terrain = new Terrain(tile.TerrainType, cropped, tile.Width, tile.Height);
                    if (!terrains.ContainsKey(terrain.Key))
                    {
                        terrains.Add(terrain.Key, terrain);
                    }
                    else
                    {
                        terrains[terrain.Key] = terrain;
                    }

                    var tiles = _session.GetTiles();
                    var offsetX = mapTile.X + i > Settings.Width - 1 ? Settings.Width - 1 : mapTile.X + i;
                    var offsetY = mapTile.Y + j > Settings.Height - 1 ? Settings.Height - 1 : mapTile.Y + j;
                    var previous = tiles[offsetX, offsetY];
                    tiles[offsetX, offsetY] = new Tile(previous.X, previous.Y, terrain.Key);
                }
            }
        }

        public void Update()
        {
            //todo:
        }

        public void Dispose()
        {
            var terrains = _session.GetTerrains();
            if (terrains != null)
            {
                foreach (var t in terrains.Values)
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
            switch (command)
            {
                case PlaceTileCommand c:
                    //todo: add a removeTile method
                    // command has image height on it, so remove all tiles over that area
                    //var terrainIndex = c.PreviousTerrain.FirstOrDefault()?.TerrainIndex;
                    //if (!terrainIndex.HasValue)
                    //    return;
                    //var terrain = Settings.Terrains[terrainIndex.Value];
                    //SetTile(c.Point, terrain);
                    break;
            }
        }
    }
}
