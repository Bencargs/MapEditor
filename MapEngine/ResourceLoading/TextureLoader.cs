using Common;
using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MapEngine.ResourceLoading
{
    public static class TextureLoader
    {
        public static IImage LoadImage(string filename)
        {
            var uri = new Uri(filename);
            var bitmap = new BitmapImage(uri);
            var image = new WriteableBitmap(bitmap);
            return new WpfImage(image);
        }

        public static IAnimation LoadAnimation(string filename, int? framerate = null)
        {
            var uri = new Uri(filename);
            var decoder = new GifBitmapDecoder(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
            var frames = decoder.Frames.Select(x => new WriteableBitmap(x).Scale(0.99)).ToArray();// todo: remove scaling - Why does scaling fix this bug??!
            return new WpfAnimation(frames, framerate ?? 40);
        }
    }
}
