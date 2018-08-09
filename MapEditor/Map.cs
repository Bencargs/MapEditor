﻿using System;
using System.Drawing;
using Newtonsoft.Json;

namespace MapEditor
{
    public class Map : IDisposable
    {
        private readonly Tile[,] _cells;
        private readonly IGraphics _graphics;
        private const int CellSize = 20;

        [JsonConverter(typeof(JsonImageConverter))]
        public Image Background { get; set; }
        public int Width { get; }
        public int Height { get; }
        public bool ShowGrid { get; set; }

        public Map(IGraphics graphics, int width, int height, Image background = null)
        {
            _graphics = graphics;
            Width = width;
            Height = height;
            Background = background;

            _cells = new Tile[Width, Height];
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    _cells[x, y] = new Tile(x, y, CellSize, Terrain.Empty);
                }
            }
        }

        public Tile GetCell(Point point)
        {
            var x = point.X * Width / _graphics.Width;
            var y = point.Y * Height / _graphics.Height;

            if (x > Width)
                x = Width;
            else if (x < 0)
                x = 0;

            if (y > Height)
                y = Height;
            else if (y < 0)
                y = 0;

            return _cells[x, y];
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
                _graphics.DrawImage(Background, new Point(0, 0));
            }
        }

        private void RenderTiles()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var tile = _cells[x, y];
                    if (!tile.IsDirty)
                        continue;

                    tile.Render(_graphics);
                    //tile.IsDirty = false;
                }
            }
        }

        private void RenderGrid()
        {
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
        }
    }
}
