using Common;

namespace MapEngine
{
    public class Texture : ITexture
    {
        public int Width { get; }
        public int Height { get; }
        public IImage Image => _image ?? _animation.Image;

        private readonly IImage _image;
        private readonly IAnimation _animation;

        private Texture(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Texture(IImage image)
            : this(image.Width, image.Height)
        {
            _image = image;
        }

        public Texture(IAnimation animation)
            : this (animation.Width, animation.Height)
        {
            _animation = animation;
        }
    }
}
