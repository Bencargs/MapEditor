using MapEngine.Factories;
using MapEngine.Services.Effects.FluidEffect;
using MapEngine.Services.Map;
using System;
using System.Linq;

namespace MapEngine.Services.Effect
{
    public class FluidEffectService
    {
        public int Width;
        public int Height;
        public Field Velocity;
        
        // todo: move surface and type into seperate class?
        public enum Type
        {
            Solid,
            Fluid
        }

        private Type[,] _surface;
        private float _resolution;
        private float _halfResolution;
        private float[] _flow;
        private float[] _newU; // temp buffer for horizontal velocity vectors
        private float[] _newV; // temp buffer for vertical velocity vectors
        private float[] _newM; // temp buffer for fluid concentration
        private readonly MapService _mapService;

        private bool _enabled = false; // todo: there should be a better way to configure effects per map

        private const float OverRelaxation = 1.9f;

        public FluidEffectService(
            MapService mapService)
        {
            _mapService = mapService;
        }

        public void Initialise() 
        {
            // todo: if not exists?
            if (!TextureFactory.TryGetTexture(_mapService.FluidEffects.Surface, out var surfaceMap))
                return;

            _enabled = true;

            // surface image has padding around edges to avoid bounds checks
            // todo: remove this - mape should be the exact size, iterators should have a boarder
            Width = surfaceMap.Width;
            Height = surfaceMap.Height;

            var arraySize = Width * Height;
            _surface = new Type[Width, Height];
            Velocity = new Field(Width, Height);
            _newU = new float[arraySize];
            _newV = new float[arraySize];
            _newM = new float[arraySize];
            _flow = Enumerable.Repeat(1.0f, arraySize).ToArray();

            // todo: effects config instead?
            _resolution = _mapService.FluidEffects.Resolution;
            _halfResolution = 0.5f * _resolution;

            for (var x = 0; x < surfaceMap.Width; x++)
                for (var y = 0; y < surfaceMap.Height; y++)
                {
                    var value = surfaceMap.Image[x, y];

                    _surface[x, y] = value.Red == 0
                        ? Type.Solid
                        : Type.Fluid;
                }
        }

        public void SetEmitter(int x, int y, float value)
        {
            if (!_enabled) return;

            Velocity[x, y, Direction.Left] = value;
            Velocity[x, y, Direction.Up] = value;
            Velocity[x, y, Direction.Down] = value;
            Velocity[x, y, Direction.Right] = value;
            _flow[y] = 0.0f;
        }

        public void Simulate(float elapsed, int iterations)
        {
            if (!_enabled) return;

            SolveIncompressibility(iterations);

            AdvectVel(elapsed);

            AdvectSmoke(elapsed);
        }

        public byte[] GenerateBitmap()
        {
            if (!_enabled) return new byte[0];

            // Upscale up the internal model 4x to fit screen (improves update processing speed)
            var scaledMap = _flow.Scale(4, 4, Width, Height);

            // todo: remove this +/- 2 boarder hack
            var width = (Width - 2) * 4;
            var height = (Height - 2) * 4;
            var result = new byte[width * height * 4];

            var i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var val = (byte)Math.Min(255, scaledMap[x, y] * 255);

                    result[i++] = val;
                    result[i++] = val;
                    result[i++] = val;
                    result[i++] = (byte)(255 - val); // 0 opacity = don't draw anything
                }
            }
            return result;
        }


        /// <summary>
        /// Each cell should have a net 0 flow
        /// sum all inpout and output vectors into cell
        /// if there is an excess inflow, then divide it amongst all vectors and subtract
        /// /if ther is excess outflow, sum and add the average to each cell
        /// (Gauss-Seidel method)
        /// </summary>
        private void SolveIncompressibility(int numIters)
        {
            for (var iter = 0; iter < numIters; iter++)
            {
                for (var i = 1; i < Width - 1; i++)
                {
                    for (var j = 1; j < Height - 1; j++)
                    {
                        if (_surface[i, j] == Type.Solid)
                            continue;

                        // Find all fluid vectors adjacent to this cell
                        var sx0 = (int)_surface[(i - 1), j];
                        var sx1 = (int)_surface[(i + 1), j];
                        var sy0 = (int)_surface[i, j - 1];
                        var sy1 = (int)_surface[i, j + 1];
                        var s = sx0 + sx1 + sy0 + sy1;
                        if (s == 0)
                            continue;

                        // divergence - sum of all inflows or outflows into cell
                        var div = Velocity[i, j, Direction.Right] - Velocity[i, j, Direction.Left] +
                                  Velocity[i, j, Direction.Up] - Velocity[i, j, Direction.Down];

                        //if (div == 0)
                        //    continue;

                        // this optimisation to speeds up balancing cell flow
                        var p = -div / s;
                        p *= OverRelaxation;

                        Velocity[i, j, Direction.Left] -= sx0 * p;
                        Velocity[i, j, Direction.Right] += sx1 * p;
                        Velocity[i, j, Direction.Up] += sy1 * p;
                        Velocity[i, j, Direction.Down] -= sy0 * p;
                    }
                }
            }
        }

        private void AdvectVel(float dt)
        {
            // todo - move this inside Field class?
            var length = Buffer.ByteLength(_newU);
            Buffer.BlockCopy(Velocity._u, 0, _newU, 0, length);
            Buffer.BlockCopy(Velocity._v, 0, _newV, 0, length);

            // parallel for
            for (var i = 1; i < Width; i++)
            {
                for (var j = 1; j < Height; j++)
                {
                    if (_surface[i, j] == Type.Solid)
                        continue;

                    // u component
                    if (_surface[i - 1, j] != Type.Solid && j < Height - 1)
                    {
                        var x = i * _resolution;
                        var y = j * _resolution + _halfResolution;
                        var u = Velocity[i, j, Direction.Left];
                        var v = AvgV(i, j);
                        x -= dt * u;
                        y -= dt * v;
                        u = SampleU(x, y);
                        _newU[i * Height + j] = u;
                    }
                    // v component
                    if (_surface[i, j - 1] != Type.Solid && i < Width - 1)
                    {
                        var x = i * _resolution + _halfResolution;
                        var y = j * _resolution;
                        var u = AvgU(i, j);
                        var v = Velocity[i, j, Direction.Down];
                        x -= dt * u;
                        y -= dt * v;
                        v = SampleV(x, y);
                        _newV[i * Height + j] = v;
                    }
                }
            };

            Buffer.BlockCopy(_newU, 0, Velocity._u, 0, length);
            Buffer.BlockCopy(_newV, 0, Velocity._v, 0, length);
        }

        private void AdvectSmoke(float dt)
        {
            var length = Buffer.ByteLength(_newM);
            Buffer.BlockCopy(_flow, 0, _newM, 0, length);

            for (var i = 1; i < Width - 1; i++)
            {
                for (var j = 1; j < Height - 1; j++)
                {
                    if (_surface[i, j] == Type.Solid)
                        continue;

                    var u = (Velocity[i, j, Direction.Left] + Velocity[i, j, Direction.Right]) * 0.5f;
                    var v = (Velocity[i, j, Direction.Down] + Velocity[i, j, Direction.Up]) * 0.5f;
                    var x = i * _resolution + _halfResolution - dt * u;
                    var y = j * _resolution + _halfResolution - dt * v;

                    _newM[i * Height + j] = SampleM(x, y);

                }
            }
            Buffer.BlockCopy(_newM, 0, _flow, 0, length);
        }

        private float SampleU(float x, float y) => SampleField(Velocity._u, x, y, 0, _halfResolution);
        private float SampleV(float x, float y) => SampleField(Velocity._v, x, y, _halfResolution, 0);
        private float SampleM(float x, float y) => SampleField(_flow, x, y, _halfResolution, _halfResolution);

        private float SampleField(float[] field, float x, float y, float dx, float dy)
        {
            var h1 = 1.0f / _resolution;

            x = Math.Max(Math.Min(x, Width * _resolution), _resolution);
            y = Math.Max(Math.Min(y, Height * _resolution), _resolution);

            var x0 = (int)Math.Min(Math.Floor((x - dx) * h1), Width - 1);
            var tx = ((x - dx) - x0 * _resolution) * h1;
            var x1 = Math.Min(x0 + 1, Width - 1);

            var y0 = (int)Math.Min(Math.Floor((y - dy) * h1), Height - 1);
            var ty = ((y - dy) - y0 * _resolution) * h1;
            var y1 = Math.Min(y0 + 1, Height - 1);

            var sx = 1.0f - tx;
            var sy = 1.0f - ty;

            return
                sx * sy * field[x0 * Height + y0] +
                tx * sy * field[x1 * Height + y0] +
                tx * ty * field[x1 * Height + y1] +
                sx * ty * field[x0 * Height + y1];
        }

        /// <summary>
        /// Get neighbour cell's flow value
        /// This will be subtracted from neighbour and added to current cell
        /// Simulates particles flowing from one cell to another
        /// (Semi-Lagrangian Advection)
        /// </summary>
        private float AvgU(int i, int j)
            => (Velocity[i, j - 1, Direction.Left] + Velocity[i, j, Direction.Left] +
                Velocity[i, j - 1, Direction.Right] + Velocity[i, j, Direction.Right]) * 0.25f;

        private float AvgV(int i, int j)
            => (Velocity[i - 1, j, Direction.Down] + Velocity[i, j, Direction.Down] +
                Velocity[i - 1, j, Direction.Up] + Velocity[i, j, Direction.Up]) * 0.25f;
    }
}
