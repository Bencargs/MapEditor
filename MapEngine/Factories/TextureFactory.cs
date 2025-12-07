using MapEngine.ResourceLoading;
using System;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Factories
{
    public class TextureFactory
    {
        private static Dictionary<string, Texture> _textures = new Dictionary<string, Texture>(StringComparer.OrdinalIgnoreCase);

        // todo: passing in a framerate for all files in a directory is definitely not right
        public static void LoadTextures(string filepath, int? framerate = null)
        {
            foreach (var file in Directory.GetFiles(filepath, "*.gif"))
            {
                var name = Path.GetFileNameWithoutExtension(file).ToUpper();
                var animation = TextureLoader.LoadAnimation(file, framerate);
                var texture = new Texture(animation);
                _textures[name] = texture;
            }

            foreach (var file in Directory.GetFiles(filepath, "*.png"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var image = TextureLoader.LoadImage(file);
                var texture = new Texture(image);
                _textures[name] = texture;
            }
        }

        public static bool TryGetTexture(string textureId, out Texture texture)
        {
            texture = default;
            if (textureId is null)
                return false;
            
            return _textures.TryGetValue(textureId, out texture);
        }
    }
}
