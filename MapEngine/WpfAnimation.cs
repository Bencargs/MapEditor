using Common;
using System.Windows.Media.Imaging;

namespace MapEngine
{
    public class WpfAnimation : IAnimation
    {
        public int Width { get; }
        public int Height { get; }
        public IImage Image
        {
            get
            {
                _index = ++_index % Width;

                var buffer = _frames[_index];
                var image = new WriteableBitmap(buffer);
                return new WpfImage(image);
            }
        }

        private int _index;
        private readonly WriteableBitmap[] _frames;

        public WpfAnimation(WriteableBitmap[] frames)
        {
            _frames = frames;
            Width = _frames[0].PixelWidth;
            Height = _frames[0].PixelHeight;
        }
    }
}
