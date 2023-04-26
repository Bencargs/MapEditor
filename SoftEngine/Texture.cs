using Common;
using SharpDX;
using System;

namespace SoftEngine
{
    public class Texture : IImage
    {
        public byte[] Buffer { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Texture(int width, int height, byte[] buffer)
        {
            Width = width;
            Height = height;
            Buffer = buffer;
        }

        public Texture(ITexture texture)
            : this(texture.Width, texture.Height, texture.Image.Buffer)
        {
        }

        // Takes the U & V coordinates exported by Blender
        // and return the corresponding pixel color in the texture
        public Color4 Map(float tu, float tv)
        {
            // Image is not loaded yet
            if (Buffer == null)
            {
                return Color4.White;
            }
            // using a % operator to cycle/repeat the texture if needed
            int u = Math.Abs((int)(tu * Width) % Width);
            int v = Math.Abs((int)(tv * Height) % Height);

            int pos = (u + v * Width) * 4;
            byte b = Buffer[pos];
            byte g = Buffer[pos + 1];
            byte r = Buffer[pos + 2];
            byte a = Buffer[pos + 3];

            return new Color4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        public Colour this[int x, int y] 
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }

        public void Draw(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public IImage Scale(float scale)
        {
            throw new NotImplementedException();
        }

        public IImage Rotate(float angle)
        {
            throw new NotImplementedException();
        }

        public IImage Fade(float fade)
        {
            throw new NotImplementedException();
        }
    }
}
