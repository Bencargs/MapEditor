using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MapEngine
{
    public class ResourceLoader
    {
        public IImage LoadImage(string filename)
        {
            var uri = new Uri(filename);
            var bitmap = new BitmapImage(uri);
            var image = new WriteableBitmap(bitmap);
            return new WpfImage(image);
        }

        public IAnimation LoadAnimation(string filename, int framerate)
        {
            var uri = new Uri(filename);
            var decoder = new GifBitmapDecoder(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
            var frames = decoder.Frames.Select(x => new WriteableBitmap(x).Scale(0.99)).ToArray();// todo: remove scaling - Why does scaling fix this bug??!
            return new WpfAnimation(frames, framerate);
        }

        public Map LoadMap(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic mapData = JsonConvert.DeserializeObject(json);
            var textures = ((IEnumerable<dynamic>)mapData.Textures).Select(x =>
            {
                var id = (int)x.Id;
                var filepath = Path.GetFullPath($@"..\..\Content\{(string)x.Filename}");
                var extension = Path.GetExtension(filepath);
                switch (extension)
                {
                    // yuck - replace with a common interface
                    case ".png":
                        var image = LoadImage(filepath);
                        return new Texture(id, image);

                    case ".gif":
                        var animation = LoadAnimation(filepath, (int)x.Framerate);
                        return new Texture(id, animation);

                    default:
                        return null;
                }
            }).ToDictionary(k => k.Id, v => v);

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
            var tiles = Make2DArray(rawTileData, (int) mapData.TileWidth, (int) mapData.TileHeight);

            var map = new Map
            {
                Width = mapData.Width,
                Height = mapData.Height,
                Tiles = tiles,
                Textures = textures
            };

            return map;
        }

        private static T[,] Make2DArray<T>(T[] input, int width, int height)
        {
            T[,] output = new T[width, height];
            for (int x = 0; x < width; x++) 
            {
                for (int y = 0; y < height; y++)
                {
                    output[x, y] = input[x + y * width];
                }
            }
            return output;
        }
    }
}
