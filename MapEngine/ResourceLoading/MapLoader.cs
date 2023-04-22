using Common;
using MapEngine.Services.Effects.FluidEffect;
using MapEngine.Services.Effects.WaveEffect;
using MapEngine.Services.Map;
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

            var tiles = LoadTiles(mapData);

            var teams = LoadTeams(mapData);

            var map = new Map
            {
                Width = mapData.Width,
                Height = mapData.Height,
                Teams = teams,
                Tiles = tiles,
                FluidEffects = LoadFluidEffects(mapData),
                WaveEffects = LoadWaveEffects(mapData)
            };

            return map;
        }

        private static Team[] LoadTeams(dynamic mapData)
        {
            var teams = ((IEnumerable<dynamic>)mapData.Teams).Select(x => new Team { Id = x.Id, Name = x.Name }).ToArray();
            return teams;
        }

        private static Tile[,] LoadTiles(dynamic mapData)
        {
            var rawTileData = ((IEnumerable<dynamic>)mapData.Tiles).Select(x => new Tile
            {
                Id = x.Id,
                Location = new Vector2((int)x.Location.X, (int)x.Location.Y),
                TextureId = x.TextureId,
                Type = x.Type
            }).ToArray();
            var tiles = rawTileData.To2DArray((int)mapData.TileWidth, (int)mapData.TileHeight);
            return tiles;
        }

        private static FluidEffects LoadFluidEffects(dynamic mapData)
        {
            var fluidConfig = ((IEnumerable<dynamic>)mapData.Effects)?.FirstOrDefault(x => x.Name == "FluidEffect");
            return new FluidEffects
            {
                Resolution = (float)(fluidConfig?.Resolution ?? 0.01f),
                Surface = fluidConfig?.Surface ?? ""
            };
        }

        private static WaveEffects LoadWaveEffects(dynamic mapData)
        {
            var waveConfig = ((IEnumerable<dynamic>)mapData.Effects)?.FirstOrDefault(x => x.Name == "WaveEffect");
            return new WaveEffects
            {
                Resolution = (float)(waveConfig?.Resolution ?? 0.01f),
                Mass = (float)(waveConfig?.Mass ?? 0.01f),
                MaxHeight = (float)(waveConfig?.MaxHeight ?? 0.01f),
                Sustain = (float)(waveConfig?.Sustain ?? 0.01f),
                Surface = waveConfig?.Surface ?? ""
            };
        }
    }
}
