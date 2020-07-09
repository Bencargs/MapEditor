using MapEngine.ResourceLoading;
using System;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Factories
{
    public static class TextureFactory
    {
        public static Dictionary<string, Texture> _textures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);

        public static void LoadTextures(string filepath)
        {
            foreach (var file in Directory.GetFiles(filepath, "*.gif"))
            {
                var name = Path.GetFileNameWithoutExtension(file).ToUpper();
                var animation = TextureLoader.LoadAnimation(file);
                var texture = new Texture(animation);
                _textures.Add(name, texture);
            }

            foreach (var file in Directory.GetFiles(filepath, "*.png"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var image = TextureLoader.LoadImage(file);
                var texture = new Texture(image);
                _textures.Add(name, texture);
            }
        }

        public static bool TryGetTexture(string textureId, out Texture texture)
        {
            return _textures.TryGetValue(textureId, out texture);
        }
    }
}
