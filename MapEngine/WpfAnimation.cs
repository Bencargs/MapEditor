using Common;
using System;
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
                var now = DateTime.Now;
                if (ShouldAnimate(now, out var newFrame))
                {
                    _previous = now;
                    _index = newFrame;
                }

                var buffer = _frames[_index];
                return new WpfImage(buffer);
            }
        }

        private int _index;
        private readonly WriteableBitmap[] _frames;
        private readonly int _frameRate;
        private DateTime _previous;

        public WpfAnimation(WriteableBitmap[] frames, int frameRate)
        {
            _frames = frames;
            Width = _frames[0].PixelWidth;
            Height = _frames[0].PixelHeight;

            _frameRate = frameRate;
            _previous = DateTime.Now;
        }

        private bool ShouldAnimate(DateTime now, out int nextFrame)
        {
            nextFrame = _index;

            var delta = (now - _previous).TotalMilliseconds;
            var drawFrames = (int) delta / _frameRate;
            if (drawFrames > 0)
            {
                nextFrame = (_index + drawFrames) % Width;
                return true;
            }
            return false;
        }
    }
}
