using Common;
using MapEngine.Factories;
using MapEngine.ResourceLoading;

namespace MapEngine.Handlers
{
    public class MapHandler
    {
        private Map _map;

        public void Initialise(string mapFile)
        {
            // In future this would load only relevant map textures
            //_textures.LoadTextures(@"C:\Source\MapEditor\MapEngine\Content\Textures\");

            _map = MapLoader.LoadMap(mapFile);
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            DrawTiles(viewport, graphics, _map.Tiles);
        }

        private void DrawTiles(Rectangle viewport, IGraphics graphics, Tile[,] tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile == null)
                    continue;

                if (TextureFactory.TryGetTexture(tile.TextureId, out var texture))
                {
                    var area = new Rectangle((int)tile.Location.X, (int)tile.Location.Y, texture.Width, texture.Height);
                    area.Translate(viewport.X, viewport.Y);
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }
    }
}
