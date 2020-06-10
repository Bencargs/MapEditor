using Common;

namespace MapEngine
{
    public class Texture : ITexture
    {
        public int Id { get; }
        public int Width { get; }
        public int Height { get; }
        public IImage Image => _image ?? _animation.Image;

        private readonly IImage _image;
        private readonly IAnimation _animation;

        private Texture(int id, int width, int height)
        {
            Id = id;
            Width = width;
            Height = height;
        }

        public Texture(int id, IImage image)
            : this(id, image.Width, image.Height)
        {
            _image = image;
        }

        public Texture(int id, IAnimation animation)
            : this (id, animation.Width, animation.Height)
        {
            _animation = animation;
        }
    }
}
