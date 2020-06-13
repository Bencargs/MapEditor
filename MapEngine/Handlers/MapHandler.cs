using Common;

namespace MapEngine.Handlers
{
    public class MapHandler
    {
        private Map _map;
        private TextureHandler _textures = new TextureHandler();

        public MapHandler(TextureHandler textures)
        {
            _textures = textures;
        }

        public void Init(string mapFile)
        {
            // Infuture this would load only relevant map textures
            //_textures.LoadTextures(@"C:\Source\MapEditor\MapEngine\Content\Textures\");

            _map = ResourceLoader.LoadMap(mapFile);
        }

        public void Render(IGraphics graphics)
        {
            DrawTiles(graphics, _map.Tiles);
        }

        private void DrawTiles(IGraphics graphics, Tile[,] tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile == null)
                    continue;

                if (_textures.TryGetTexture(tile.TextureId, out var texture))
                {
                    var area = new Rectangle(tile.Location.X, tile.Location.Y, texture.Width, texture.Height);
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }
    }
}
