using Common;
using System;
using System.Linq;
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

                return _frames[_index];
            }
        }

        private int _index;
        private readonly WpfImage[] _frames;
        private readonly int _frameRate;
        private DateTime _previous;

        public WpfAnimation(WriteableBitmap[] frames, int frameRate)
        {
            _frames = frames.Select(x => new WpfImage(x)).ToArray();
            Width = _frames[0].Width;
            Height = _frames[0].Height;

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
