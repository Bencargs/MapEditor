using Common;
using Common.Entities;
using MapEngine.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MapEngine
{
    public static class ResourceLoader
    {
        public static IImage LoadImage(string filename)
        {
            var uri = new Uri(filename);
            var bitmap = new BitmapImage(uri);
            var image = new WriteableBitmap(bitmap);
            return new WpfImage(image);
        }

        public static IAnimation LoadAnimation(string filename, int? framerate = null)
        {
            var uri = new Uri(filename);
            var decoder = new GifBitmapDecoder(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
            var frames = decoder.Frames.Select(x => new WriteableBitmap(x).Scale(0.99)).ToArray();// todo: remove scaling - Why does scaling fix this bug??!
            return new WpfAnimation(frames, framerate ?? 40);
        }

        public static Entity LoadUnit(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic unitData = JsonConvert.DeserializeObject(json);

            var entity = new Entity();

            var location = unitData.Location;
            if (location != null)
            {
                entity.AddComponent(new LocationComponent
                {
                    Location = new Point((int)location.X, (int)location.Y)
                });
            }

            var image = unitData.Image;
            if (image != null)
            {
                entity.AddComponent(new ImageComponent
                {
                    TextureId = (string)image.TextureId
                });
            }

            return entity;
        }

        public static Map LoadMap(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic mapData = JsonConvert.DeserializeObject(json);

            var rawTileData = ((IEnumerable<dynamic>)mapData.Tiles).Select(x =>
            {
                return new Tile
                {
                    Id = x.Id,
                    Location = new Common.Point((int)x.Location.X, (int)x.Location.Y),
                    TextureId = x.TextureId,
                    Type = x.Type
                };
            }).ToArray();
            var tiles = EnumerableEx.Make2DArray(rawTileData, (int) mapData.TileWidth, (int) mapData.TileHeight);

            var map = new Map
            {
                Width = mapData.Width,
                Height = mapData.Height,
                Tiles = tiles,
            };

            return map;
        }
    }
}
