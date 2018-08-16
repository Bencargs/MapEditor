using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor
{
    public enum Terrain
    {
        Empty = 0,
        Water,
        Land,
        ImpassableLand,
    }

    public class Tile : IDisposable
    {
        public bool IsDirty { get; set; }
        public Terrain Terrain { get; set; }
        public Color Colour
        {
            get
            {
                switch (Terrain)
                {
                    case Terrain.Empty:
                        return Color.Transparent;
                    case Terrain.Water:
                        return SystemColors.ActiveCaption;
                    case Terrain.Land:
                        return Color.Green;
                    case Terrain.ImpassableLand:
                        return Color.LightSlateGray;
                    default:
                        return Color.Empty;
                }
            }
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Size { get; set; }
        public Image Image { get; set; }

        public Tile(int x, int y, int size, Terrain terrain)
        {
            X = x;
            Y = y;
            Size = size;
            Terrain = terrain;
            IsDirty = true;
        }

        public void Render(IGraphics graphics)
        {
            if (Image != null)
            {
                graphics.DrawImage(Image, new Point(X, Y));
            }
        }

        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
