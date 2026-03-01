using Common;
using MapEngine.Factories;
using MapEngine.ResourceLoading;
using MapEngine.Services.Map;

namespace MapEngine.Handlers
{
    public class MapHandler
    {
        private readonly MapService _mapService;

        public MapHandler(MapService mapService)
        {
            _mapService = mapService;
        }

        public void Initialise(string mapFile)
        {
            // todo: create directory of specific map textures - rather than loading everything
            TextureFactory.LoadTextures(@"C:\src\MapEditor\MapEngine\Content\Textures");

            var map = MapLoader.LoadMap(mapFile);
            _mapService.Initialise(map);
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            DrawTiles(viewport, graphics, _mapService.TextureTiles);
        }

        private void DrawTiles(Rectangle viewport, IGraphics graphics, Tile[,] tiles)
        {
            foreach (var tile in tiles)
            {
                if (!viewport.Contains(tile.Location))
                    continue;

                if (!TextureFactory.TryGetTexture(tile.TextureId, out var texture)) 
                    continue;
                
                var baseX = (int)tile.Location.X + viewport.X;
                var baseY = (int)tile.Location.Y + viewport.Y;
                var area = new Rectangle((int)tile.Location.X, (int)tile.Location.Y, texture.Width, texture.Height);
                area.Translate(viewport.X, viewport.Y);
                    
                if (tile.SubSurfaceTextureId != null &&
                    TextureFactory.TryGetTexture(tile.SubSurfaceTextureId, out var subSurfaceTexture))
                {
                    DrawTiled(graphics, subSurfaceTexture, baseX, baseY, texture.Width, texture.Height);
                }
                    
                if (tile.SurfaceTextureId != null &&
                    TextureFactory.TryGetTexture(tile.SurfaceTextureId, out var surfaceTexture))
                {
                    DrawTiled(graphics, surfaceTexture, baseX, baseY, texture.Width, texture.Height);
                }
                    
                graphics.DrawImage(texture.Image, area);
            }
        }
        
        private void DrawTiled(IGraphics graphics, Texture texture, int baseX, int baseY, int coverWidth, int coverHeight)
        {
            int imgW = texture.Image.Width;
            int imgH = texture.Image.Height;

            for (int y = 0; y < coverHeight; y += imgH)
            {
                for (int x = 0; x < coverWidth; x += imgW)
                {
                    var area = new Rectangle(baseX + x, baseY + y, imgW, imgH);
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }
    }
}
