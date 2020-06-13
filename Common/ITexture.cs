namespace Common
{
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
        IImage Image { get; }
    }
}
