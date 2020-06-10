namespace Common
{
    public interface IImage
    {
        void Draw(byte[] buffer);
        int Width { get; }
        int Height { get; }

        Colour this[int x, int y]
        {
            get;
            set;
        }
    }
}
