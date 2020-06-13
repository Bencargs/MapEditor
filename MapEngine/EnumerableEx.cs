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
    }
}
