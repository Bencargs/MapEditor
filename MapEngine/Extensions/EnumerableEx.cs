using System.Collections.Generic;
using System.Linq;

namespace MapEngine
{
    public static class EnumerableEx
    {
        public static T[,] Make2DArray<T>(this T[] input, int width, int height)
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

        public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            return new Queue<T>(source);
        }

        public static bool TryDequeue<T>(this Queue<T> source, out T result)
        {
            result = default;
            if (source.Any())
            {
                result = source.Dequeue();
                return true;
            }
            return false;
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }
    }
}
