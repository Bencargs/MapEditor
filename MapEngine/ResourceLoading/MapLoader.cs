using System;
using Common;
using MapEngine.Services.Effects.FluidEffect;
using MapEngine.Services.Effects.LightingEffect;
using MapEngine.Services.Effects.WaveEffect;
using MapEngine.Services.Map;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using MapEngine.Extensions;

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
                WaveEffects = LoadWaveEffects(mapData),
                LightingEffects = LoadLightingEffects(mapData)
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
                HeightmapTextureId = x.HeightmapTextureId,
                Type = x.Type // todo: refactor this - convert an enum string to enum - more maintainable than enum int
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

        private static LightingEffects LoadLightingEffects(dynamic mapData)
        {
            var ambientLighting = Array.Empty<LightingEffects.AmbientLight>();
            var ambientConfig = ((IEnumerable<dynamic>)mapData.Effects)?.FirstOrDefault(x => x.Name == "AmbientEffect");
            if (ambientConfig != null)
            {
                ambientLighting = ((IEnumerable<dynamic>)ambientConfig.Sources)
                    .Select(x => new LightingEffects.AmbientLight
                    {
                        TextureId = x.TextureId,
                        LineOfSight = (bool)x.LineOfSight,
                        On = (int)x.On,
                        Off = (int)x.Off,
                        Location = new Vector2((int)x.Location.X, (int)x.Location.Y)
                    }).ToArray();
            }

            var diffuseLighting = Array.Empty<LightingEffects.DiffuseLight>();
            var diffuseConfig = ((IEnumerable<dynamic>)mapData.Effects)?.FirstOrDefault(x => x.Name == "DiffuseEffect");
            if (diffuseConfig != null)
            {
                diffuseLighting = ((IEnumerable<dynamic>)diffuseConfig.Sources)
                    .Select(x => new LightingEffects.DiffuseLight
                    {
                        Name = x.Name,
                        Colour = new Colour((byte)x.Colour.R, (byte)x.Colour.B, (byte)x.Colour.G, (byte)x.Colour.A),
                        TransitionType =  EnumEx.ParseOrDefault<LightingEffects.TransitionType>((string) x.TransitionType),
                        On = (int)x.On,
                        Off = (int)x.Off,
                    }).ToArray();
            }

            return new LightingEffects
            {
                Ambient = ambientLighting,
                Diffuse = diffuseLighting,
            };
        }
    }
}
