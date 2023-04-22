using Common;
using MapEngine.Services.Effects.WaveEffect;
using System;
using System.Collections.Generic;
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

        public static float[,] Scale<T>(this T[] self, Func<T, float> selector, float scaleX, float scaleY, int width, int height)
        {
            int newWidth = (int)((width) * scaleX);
            int newHeight = (int)((height) * scaleY);
            var scaledArray = new float[newWidth, newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float gx = ((float)x) / newWidth * (width - 1);
                    float gy = ((float)y) / newHeight * (height - 1);
                    int gxi = (int)gx;
                    int gyi = (int)gy;

                    var c00 = selector(self[(gxi) * (height) + (gyi)]);
                    var c10 = selector(self[(gxi + 1) * (height) + (gyi)]);
                    var c01 = selector(self[(gxi) * (height) + (gyi + 1)]);
                    var c11 = selector(self[(gxi + 1) * (height) + (gyi + 1)]);

                    var value = Blerp(c00, c10, c01, c11, gx - gxi, gy - gyi);
                    scaledArray[x, y] = value;
                }
            }

            return scaledArray;
        }

        public static float[,] Scale<T>(this T[,] self, Func<T, float> selector, float scaleX, float scaleY, int width, int height)
        {
            int newWidth = (int)((width) * scaleX);
            int newHeight = (int)((height) * scaleY);
            var scaledArray = new float[newWidth, newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float gx = ((float)x) / newWidth * (width - 1);
                    float gy = ((float)y) / newHeight * (height - 1);
                    int gxi = (int)gx;
                    int gyi = (int)gy;

                    var c00 = selector(self[gxi, gyi]);
                    var c10 = selector(self[gxi + 1, gyi]);
                    var c01 = selector(self[gxi, gyi + 1]);
                    var c11 = selector(self[gxi + 1, gyi + 1]);

                    var value = Blerp(c00, c10, c01, c11, gx - gxi, gy - gyi);
                    scaledArray[x, y] = value;
                }
            }

            return scaledArray;
        }

        public static float Blerp(float c00, float c10, float c01, float c11, float tx, float ty)
            => Lerp(Lerp(c00, c10, tx), Lerp(c01, c11, tx), ty);

        public static float Lerp(float s, float e, float t)
            => s + (e - s) * t;
    }
}
