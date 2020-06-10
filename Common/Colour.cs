namespace Common
{
    public class Colour
    {
        public byte Red { get; }
        public byte Blue { get; }
        public byte Green { get; }
        public byte Alpha { get; }

        public Colour(byte red, byte blue, byte green, byte alpha)
        {
            Red = red;
            Blue = blue;
            Green = green;
            Alpha = alpha;
        }
    }
}
