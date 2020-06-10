using Common;

namespace MapEngine
{
    public class Scene
    {
        private IGraphics _graphics;
        private readonly ResourceLoader _loader;
        private Map _map;

        public Scene(IGraphics graphics)
        {
            _graphics = graphics;
            _loader = new ResourceLoader();
        }

        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap1.json";

            _map = _loader.LoadMap(mapFilename);
        }

        public void Display()
        {
            Render();
        }

        private void Update()
        {
            
        }

        private void Render()
        {
            _graphics.Clear();

            DrawTiles(_map.Tiles);

            _graphics.Render();
        }

        private void DrawTiles(Tile[,] tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile == null)
                    continue;

                if (_map.Textures.TryGetValue(tile.TextureId, out var texture))
                {
                    var area = new Rectangle(tile.Location.X, tile.Location.Y, texture.Width, texture.Height);
                    _graphics.DrawImage(texture.Image, area);
                }
            }
        }

        private void ProcessInput()
        {

        }
    }
}
