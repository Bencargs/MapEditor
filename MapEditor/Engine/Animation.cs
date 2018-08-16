using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor.Engine
{
    public class Animation : IDisposable
    {
        private readonly List<Bitmap> _animation;
        private int _i;

        public Animation(List<Image> animation)
        {
            _animation = new List<Bitmap>();
            foreach (var a in animation)
            {
                var bitmap = new Bitmap(a);
                bitmap.MakeTransparent(Color.Fuchsia);
                _animation.Add(bitmap);
            }
        }

        public Image GetImage(double elapsed)
        {
            return null;//todo:
        }

        public Image Next()
        {
            if (_i >= _animation.Count)
                _i = 0;

            return _animation[_i++];
        }

        public void Dispose()
        {
            if ( _animation != null)
            {
                foreach (var a in _animation)
                {
                    a?.Dispose();
                }
            }
        }
    }
}
