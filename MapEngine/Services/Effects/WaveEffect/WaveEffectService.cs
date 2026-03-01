using MapEngine.Factories;
using MapEngine.Services.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;

namespace MapEngine.Services.Effects.WaveEffect
{
    public class WaveEffectService
    {
        private const int MapPixelsPerSimCell = 4;

        // When a cell is below these, we stop simulating
        private const float SleepThreshold = 0.0025f;
        private const float DrawThreshold = 0.001f;

        // todo: this sucks - figure out why this is necessary and fix
        // Nudge overlay alignment (your screenshot suggests: too far right and too high)
        private readonly int _offsetX = -80; // left
        private readonly int _offsetY = 0;

        private readonly MapService _mapService;
        private readonly MessageHub _messageHub; // todo: emit wave particle emitters along shoreline

        private bool _enabled;

        // Viewport size
        private int _mapWidth, _mapHeight;

        // Mask (water surface image at a 4x smaller than map
        private int _maskWidth, _maskHeight;

        // Sim (cells at a 4x smaller resolution than mask
        private int _simWidth, _simHeight, _simLen;

        // Sim state
        private bool[] _isWater = Array.Empty<bool>();
        private float[] _height = Array.Empty<float>();
        private float[] _velocity = Array.Empty<float>();
        private float[] _hNext = Array.Empty<float>();
        private float[] _vNext = Array.Empty<float>();

        // Active sets
        private bool[] _isActive = Array.Empty<bool>();
        private int[] _active = Array.Empty<int>();
        private int _activeCount;

        private bool[] _willBeActive = Array.Empty<bool>();
        private int[] _nextActive = Array.Empty<int>();
        private int _nextActiveCount;

        // Output buffer
        private byte[] _rgba = Array.Empty<byte>();

        private readonly List<WaveEmitter> _emitters = new List<WaveEmitter>();

        public WaveEffectService(MapService mapService, MessageHub messageHub)
        {
            _mapService = mapService;
            _messageHub = messageHub;
        }

        public void Initialise()
        {
            if (!TextureFactory.TryGetTexture(_mapService.WaveEffects.Surface, out var surfaceMap))
            {
                _enabled = false;
                return;
            }

            _enabled = true;

            _mapWidth = _mapService.Width;
            _mapHeight = _mapService.Height;
            _maskWidth = surfaceMap.Width;
            _maskHeight = surfaceMap.Height;

            _rgba = new byte[_mapWidth * _mapHeight * 4];

            _simWidth = (_maskWidth + MapPixelsPerSimCell - 1) / MapPixelsPerSimCell;
            _simHeight = (_maskHeight + MapPixelsPerSimCell - 1) / MapPixelsPerSimCell;
            _simLen = _simWidth * _simHeight;

            _isWater = new bool[_simLen];
            _height = new float[_simLen];
            _velocity = new float[_simLen];
            _hNext = new float[_simLen];
            _vNext = new float[_simLen];

            _isActive = new bool[_simLen];
            _willBeActive = new bool[_simLen];

            // Build water mask (ANY land pixel in block => land)
            for (int simY = 0; simY < _simHeight; simY++)
            {
                int y0 = simY * MapPixelsPerSimCell;
                int y1 = Math.Min(y0 + MapPixelsPerSimCell, _maskHeight);

                for (int simX = 0; simX < _simWidth; simX++)
                {
                    int x0 = simX * MapPixelsPerSimCell;
                    int x1 = Math.Min(x0 + MapPixelsPerSimCell, _maskWidth);

                    bool isLand = false;
                    for (int y = y0; y < y1 && !isLand; y++)
                    {
                        for (int x = x0; x < x1; x++)
                        {
                            isLand = surfaceMap.Image[x, y].Red == 255;
                        }
                    }

                    int i = (simY * _simWidth) + simX;
                    _isWater[i] = !isLand;
                }
            }

            _active = new int[_simLen];
            _nextActive = new int[_simLen];

            _activeCount = 0;
        }

        //todo: resource loading
        //todoL file definition
        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            var location = entity.GetComponent<LocationComponent>();
            var movement = entity.GetComponent<MovementComponent>();
            if (location == null || movement == null) return;
            
            
            // var location = entity.Location();
            // var index = MapPixelsToSimIndex((int)location.X, (int)location.Y);
            // if (!_isWater[index])
            //     return;
            
            // if (!entity.IsMoving())
            //     return;
            
            // var waveEffect = entity.GetComponents<EffectsComponent>()
            //     .FirstOrDefault(x => x.EffectType == "Wave");
            //if (waveEffect == null) return;

            _emitters.Add(new WaveEmitter
            {
                Entity = command.Entity,
                Strength = 100,
            });
        }
        
        public void Handle(DestroyEntityCommand command)
        {
            var emitter = _emitters.FirstOrDefault(x => x.Entity.Id == command.Entity.Id);
            _emitters.Remove(emitter);
        }
        
        public void SetHeight(int mapX, int mapY, float value)
        {
            if (!_enabled) return;

            int simIndex = MapPixelsToSimIndex(mapX, mapY);
            if (simIndex < 0) return;
            
            // Add energy: velocity gives nice ripples, height nudge makes it visible immediately.
            _velocity[simIndex] += value;
            _height[simIndex] = Math.Max(-_mapHeight, Math.Min(_mapHeight, _height[simIndex] + value * 0.25f));

            Activate(simIndex);
            ActivateNeighbors(simIndex);
        }
        
        private int MapPixelsToSimIndex(int mapX, int mapY)
        {
            // Convert from full-map pixels to mask pixels (mask is a crop of the full map).
            int maskX = mapX - _offsetX;
            int maskY = mapY - _offsetY;

            if ((uint)maskX >= (uint)_maskWidth || (uint)maskY >= (uint)_maskHeight)
                return -1;

            // Convert mask pixel -> sim cell (rounded to nearest cell center).
            int simX = (maskX + (MapPixelsPerSimCell / 2)) / MapPixelsPerSimCell;
            int simY = (maskY + (MapPixelsPerSimCell / 2)) / MapPixelsPerSimCell;

            if ((uint)simX >= (uint)_simWidth || (uint)simY >= (uint)_simHeight)
                return -1;

            int index = (simY * _simWidth) + simX;
            if (!_isWater[index]) return -1;

            return index;
        }

        public void Update()
        {
            if (!_enabled) return;
            foreach (var emitter in _emitters)
            {
                var location = emitter.Entity.Location();
                var maskX = (int)location.X / 4 + _offsetX;
                var maskY = (int)location.Y / 4 + _offsetY;
                
                var index = MapPixelsToSimIndex(maskX, maskY);
                if (index >= 0 && _isWater[index] && emitter.ShouldEmit())
                {
                    SetHeight(maskX, maskY, emitter.Strength);
                }
            }
            
            SimulateWaves();
        }

        private void SimulateWaves()
        {
            if (_activeCount == 0) return;

            float mass = 1f / Math.Max(_mapService.WaveEffects.Mass, 0.0001f);
            float sustain = 1f / Math.Max(_mapService.WaveEffects.Sustain, 0.0001f);
            float resolution = Math.Max(_mapService.WaveEffects.Resolution, 0.0001f);

            _nextActiveCount = 0;

            float total = 0f;
            int totalCount = 0;

            for (int k = 0; k < _activeCount; k++)
            {
                int cell = _active[k];
                _isActive[cell] = false;

                int x = cell % _simWidth;
                int y = cell / _simWidth;

                float sum = 0f;
                int count = 0;

                Add(x - 1, y);
                Add(x + 1, y);
                Add(x, y - 1);
                Add(x, y + 1);
                Add(x - 1, y - 1);
                Add(x + 1, y - 1);
                Add(x - 1, y + 1);
                Add(x + 1, y + 1);
                
                float currentHeight = _height[cell];
                float currentVelocity = _velocity[cell];

                float avg = (count > 0) ? (sum / count) : currentHeight;

                float accel = -((currentHeight - avg) * mass);
                accel -= (currentVelocity * sustain);

                currentVelocity += accel;
                currentHeight += currentVelocity / resolution;

                currentHeight = Math.Max(-_mapHeight, Math.Min(_mapHeight, currentHeight));

                _vNext[cell] = currentVelocity;
                _hNext[cell] = currentHeight;

                total += currentHeight;
                totalCount++;

                if (Math.Abs(currentHeight) > SleepThreshold || Math.Abs(currentVelocity) > SleepThreshold)
                {
                    MarkWillBeActive(cell);
                    ActivateNeighborsWillBeActive(cell);
                }

                void Add(int nx, int ny)
                {
                    if ((uint)nx >= (uint)_simWidth || (uint)ny >= (uint)_simHeight) return;
                    int ni = (ny * _simWidth) + nx;
                    if (!_isWater[ni]) return;
                    sum += _height[ni];
                    count++;
                }
            }

            if (totalCount > 0)
            {
                float mean = total / totalCount;
                for (int k = 0; k < _nextActiveCount; k++)
                {
                    int i = _nextActive[k];
                    _hNext[i] -= mean;
                }
            }

            for (int k = 0; k < _nextActiveCount; k++)
            {
                int i = _nextActive[k];

                _height[i] = _hNext[i];
                _velocity[i] = _vNext[i];

                _willBeActive[i] = false;

                _isActive[i] = true;
                _active[k] = i;
            }

            _activeCount = _nextActiveCount;
        }

        public byte[] GenerateBitmap()
        {
            if (!_enabled) return _rgba;

            for (int i = 0; i < _activeCount; i++)
            {
                int cell = _active[i];
                float height = _height[cell];

                if (Math.Abs(height) < DrawThreshold)
                    continue;

                DrawCellRect(cell, height);
            }

            return _rgba;
        }

        private void Activate(int i)
        {
            if (_isActive[i]) return;
            _isActive[i] = true;
            _active[_activeCount++] = i;
        }

        private void ActivateNeighbors(int i)
        {
            int x = i % _simWidth;
            int y = i / _simWidth;

            TryActivate(x - 1, y);
            TryActivate(x + 1, y);
            TryActivate(x, y - 1);
            TryActivate(x, y + 1);
            TryActivate(x - 1, y - 1);
            TryActivate(x + 1, y - 1);
            TryActivate(x - 1, y + 1);
            TryActivate(x + 1, y + 1);

            void TryActivate(int nx, int ny)
            {
                if ((uint)nx >= (uint)_simWidth || (uint)ny >= (uint)_simHeight) return;
                int ni = (ny * _simWidth) + nx;
                if (!_isWater[ni]) return;
                Activate(ni);
            }
        }

        private void MarkWillBeActive(int i)
        {
            if (_willBeActive[i]) return;
            _willBeActive[i] = true;
            _nextActive[_nextActiveCount++] = i;
        }

        private void ActivateNeighborsWillBeActive(int i)
        {
            int x = i % _simWidth;
            int y = i / _simWidth;

            TryMark(x - 1, y);
            TryMark(x + 1, y);
            TryMark(x, y - 1);
            TryMark(x, y + 1);
            TryMark(x - 1, y - 1);
            TryMark(x + 1, y - 1);
            TryMark(x - 1, y + 1);
            TryMark(x + 1, y + 1);

            void TryMark(int nx, int ny)
            {
                if ((uint)nx >= (uint)_simWidth || (uint)ny >= (uint)_simHeight) return;
                int ni = (ny * _simWidth) + nx;
                if (!_isWater[ni]) return;
                MarkWillBeActive(ni);
            }
        }

        private void DrawCellRect(int simIndex, float h)
        {
            float signed = Math.Max(-1f, Math.Min(1f, h / _mapService.WaveEffects.MaxHeight));
            float abs = Math.Abs(signed);
            byte alpha = (byte)Math.Max(0, Math.Min(255, abs * 255f));

            int simX = simIndex % _simWidth;
            int simY = simIndex / _simWidth;

            int x0 = (simX * _mapWidth) / _simWidth;
            int x1 = ((simX + 1) * _mapWidth) / _simWidth;
            int y0 = (simY * _mapHeight) / _simHeight;
            int y1 = ((simY + 1) * _mapHeight) / _simHeight;

            x0 += _offsetX; x1 += _offsetX;
            y0 += _offsetY; y1 += _offsetY;

            // Clamp
            if (x0 < 0) x0 = 0;
            if (y0 < 0) y0 = 0;
            if (x1 > _mapWidth) x1 = _mapWidth;
            if (y1 > _mapHeight) y1 = _mapHeight;
            if (x0 >= x1 || y0 >= y1) return;

            for (int y = y0; y < y1; y++)
            {
                int p = ((y * _mapWidth) + x0) * 4;
                for (int x = x0; x < x1; x++)
                {
                     _rgba[p + 0] = 255;
                     _rgba[p + 1] = 255;
                     _rgba[p + 2] = 255;
                     _rgba[p + 3] = alpha;
                    
                    p += 4;
                }
            }
        }
    }
}
