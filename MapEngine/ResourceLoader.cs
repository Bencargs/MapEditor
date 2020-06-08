using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapEngine
{
    public class ResourceLoader
    {
        public IImage LoadImage(string filename)
        {
            var uri = new Uri(filename);
            var bitmap = new BitmapImage(uri);
            var image = new WriteableBitmap(bitmap);
            return new WpfImage(image);
        }

        public IAnimation LoadAnimation(string filename)
        {
            var uri = new Uri(filename);
            var decoder = new GifBitmapDecoder(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
            var frames = decoder.Frames.Select(x => new WriteableBitmap(x).Scale(0.99)).ToArray();// todo: remove scaling - Why does scaling fix this bug??!
            return new WpfAnimation(frames);
        }
    }
}
