namespace Common
{
    public interface IAnimation
    {
        int Width { get; }
        int Height { get; }
        IImage Image { get; }
    }
}
