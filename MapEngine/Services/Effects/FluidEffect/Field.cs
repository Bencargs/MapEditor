using System;

namespace MapEngine.Services.Effects.FluidEffect
{
    public class Field
    {
        public float[] _v;
        public float[] _u;

        private readonly int _height;

        public Field(int width, int height)
        {
            _height = height;
            var arraySize = width * height;
            _u = new float[arraySize];
            _v = new float[arraySize];
        }

        /// <summary>
        /// Imagine a gid of cells where each cell has 4 input/output vectors
        ///      v[i,j+1]
        ///        ┌─↕─┐
        /// u[i,j] ↔   ↔ u[i+1, j]
        ///        └─↕─┘
        ///       v[i,j]
        /// </summary>
        public float this[int i, int j, Direction direction]
        {
            get
            {
                //2.1%
                switch (direction)
                {
                    case Direction.Up: return _v[i * _height + j + 1];
                    case Direction.Down: return _v[i * _height + j];
                    case Direction.Left: return _u[i * _height + j];
                    case Direction.Right: return _u[(i + 1) * _height + j];
                    default: throw new NotImplementedException();
                }
            }
            set
            {
                switch (direction)
                {
                    case Direction.Up: _v[i * _height + j + 1] = value; break;
                    case Direction.Down: _v[i * _height + j] = value; break;
                    case Direction.Left: _u[i * _height + j] = value; break;
                    case Direction.Right: _u[(i + 1) * _height + j] = value; break;
                };
            }
        }

        // todo: when setting velocity, update it, and add it to an enumerable queue
    }
}
