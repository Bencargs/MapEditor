﻿using System.Collections.Generic;
using System.Linq;

namespace MapEngine
{
    public static class EnumerableEx
    {
        public static T[,] To2DArray<T>(this T[] input, int width, int height)
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

        public static T[,] To2DArray<T>(this IEnumerable<IEnumerable<T>> source)
        {
            int max = source.Select(l => l).Max(l => l.Count());
            var result = new T[source.Count(), max];

            var x = 0;
            foreach (var i in source)
            {
                var y = 0;
                foreach (var j in i)
                {
                    result[x, y] = j;
                    y++;
                }
                x++;
            }

            return result;
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            return new Queue<T>(source);
        }

        public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> source)
        {
            foreach (var s in source)
            {
                queue.Enqueue(s);
            }
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
