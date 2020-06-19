using Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace MapEngine.ResourceLoading
{
    public static class MapLoader
    {
        public static Map LoadMap(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic mapData = JsonConvert.DeserializeObject(json);

            var rawTileData = ((IEnumerable<dynamic>)mapData.Tiles).Select(x =>
            {
                return new Tile
                {
                    Id = x.Id,
                    Location = new Vector2((int)x.Location.X, (int)x.Location.Y),
                    TextureId = x.TextureId,
                    Type = x.Type
                };
            }).ToArray();           

            var teams = ((IEnumerable<dynamic>)mapData.Teams).Select(x => new Team { Id = x.Id, Name = x.Name }).ToArray();

            var tiles = rawTileData.Make2DArray((int)mapData.TileWidth, (int)mapData.TileHeight);

            var map = new Map
            {
                Width = mapData.Width,
                Height = mapData.Height,
                Teams = teams,
                Tiles = tiles,
            };

            return map;
        }
    }
}
