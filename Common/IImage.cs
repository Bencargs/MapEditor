namespace Common
{
    public interface IImage
    {
        int Width { get; }
        int Height { get; }
        byte[] Buffer { get; }

        Colour this[int x, int y]
        {
            get;
            set;
        }

        void Draw(byte[] buffer);
        IImage Scale(float scale);
        IImage Rotate(float angle);
    }
}
