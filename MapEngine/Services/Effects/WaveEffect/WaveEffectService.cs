using Autofac.Features.Indexed;
using Common;
using MapEngine.Factories;
using MapEngine.Services.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;

namespace MapEngine.Services.Effects.WaveEffect
{
    public class WaveEffectService
    {
        // Simulation dimentions
        private int _width;
        private int _height;

        private float _mass = 0.1f;
        private float _maxHeight = 500f;
        private float _resolution = 5f;
        private float _sustain = 1000f;

        private WaterParticle[,] _grid;
        private readonly List<(int X, int Y)> _fluid = new List<(int X, int Y)>(); // holds regions of the grid that contains a fluid

        private bool _enabled = false; // todo: there should be a better way to configure effects per map
        private readonly MapService _mapService;

        public WaveEffectService(MapService mapService)
        {
            _mapService = mapService;
        }

        public void Initialise()
        {
            if (!TextureFactory.TryGetTexture(_mapService.WaveEffects.Surface, out var surfaceMap))
                return;

            _enabled = true;
            _width = surfaceMap.Width;
            _height = surfaceMap.Height;
            _mass = _mapService.WaveEffects.Mass;
            _maxHeight = _mapService.WaveEffects.MaxHeight;
            _resolution = _mapService.WaveEffects.Resolution;
            _sustain = _mapService.WaveEffects.Sustain;

            _grid = new WaterParticle[_width, _height];

            for (var y = 0; y < surfaceMap.Height; y++)
                for (var x = 0; x < surfaceMap.Width; x++) 
                {
                    var value = surfaceMap.Image[x, y];
                    if (value.Red != 0)
                        continue;
                    
                    _fluid.Add((x, y));
                    _grid[x, y] = new WaterParticle
                    {
                        Height = 0,
                        Velocity = 0,
                        Acceleration = 0,
                        Sustainability = _sustain
                    };
                }
        }

        public void SetHeight(int x, int y, float value)
        {
            if (x < 0)
                x = 0;
            else if (x > _width - 1)
                x = _width - 1;

            if (y < 0)
                y = 0;
            else if (y > _height - 1)
                y = _height - 1;

            var cell = _grid[x, y];
            if (cell == null)
                return;

            cell.Height = value;
        }

        public void Simulate()
        {
            if (!_enabled) return;

            float total_height = 0;

            foreach (var (x, y) in _fluid)
            {
                var value = _grid[x, y];

                value.Acceleration = 0;
                total_height += value.Height;

                float heights = 0;
                int num_of_part = 0;
                foreach (var particle in GetMask(x, y).Where(p => p.Value != null))
                {
                    heights += particle.Value.Height;
                    num_of_part++;
                }

                heights /= num_of_part;
                value.Acceleration += -(value.Height - heights) / _mass;
                value.Acceleration -= value.Velocity / value.Sustainability;

                if (value.Acceleration > _maxHeight)
                    value.Acceleration = _maxHeight;
                else if (value.Acceleration < -_maxHeight)
                    value.Acceleration = -_maxHeight;
            }

            float shifting = -total_height / _grid.Length;
            foreach (var (x, y) in _fluid)
            {
                var value = _grid[x, y];

                value.Velocity += value.Acceleration;

                if (value.Height + value.Velocity / _resolution > _maxHeight)
                    value.Height = _maxHeight;
                else if (value.Height + value.Velocity / _resolution <= _maxHeight && value.Height + value.Velocity / _resolution >= -_maxHeight)
                    value.Height += value.Velocity / _resolution;
                else
                    value.Height = -_maxHeight;

                value.Height += shifting;
            }
        }

        private IEnumerable<(int x, int y, WaterParticle Value)> GetMask(int x, int y)
        {
            if (x > 0)
                yield return (x - 1, y, _grid[x - 1, y]);

            if (x > 0 && y > 0)
                yield return (x - 1, y - 1, _grid[x - 1, y - 1]);

            if (y > 0)
                yield return (x, y - 1, _grid[x, y - 1]);

            if (y > 0 && x < _width - 1)
                yield return (x + 1, y - 1, _grid[x + 1, y - 1]);

            if (x < _width - 1)
                yield return (x + 1, y, _grid[x + 1, y]);

            if (x < _width - 1 && y < _height - 1)
                yield return (x + 1, y + 1, _grid[x + 1, y + 1]);

            if (y < _height - 1)
                yield return (x, y + 1, _grid[x, y + 1]);

            if (y < _height - 1 && x > 0)
                yield return (x - 1, y + 1, _grid[x - 1, y + 1]);
        }

        public byte[] GenerateBitmap()
        {
            if (!_enabled) return new byte[0];

            // Upscale up the internal model 4x to fit screen (improves update processing speed)
            var result = new byte[(_width * 4) * (_height * 4) * 4];
            var scaledMap = _grid.Scale(x => x?.Height ?? 0, 4, 4, _width, _height);
            var i = 0;
            for (int y = 0; y < _height * 4; y++)
            {
                for (int x = 0; x < _width * 4; x++)
                {
                    var cell = scaledMap[x, y];
                    if (cell == 0)
                    {
                        i += 4;
                        continue;
                    }

                    // todo: this is insanity - not sure what my thought process was here - fix!!
                    var value = (byte)((cell + _maxHeight) / (_maxHeight * 1.5f / 255f));

                    result[i++] = value;
                    result[i++] = value;
                    result[i++] = value;
                    result[i++] = (byte)(255 - value);
                }
            }
            return result;
        }
    }
}
