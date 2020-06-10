namespace Common
{
    public interface ITexture
    {
        int Id { get; }
        int Width { get; }
        int Height { get; }
        IImage Image { get; }
    }
}
