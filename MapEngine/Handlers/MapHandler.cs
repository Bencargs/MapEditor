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

                if (TextureFactory.TryGetTexture(tile.TextureId, out var texture))
                {
                    var area = new Rectangle((int)tile.Location.X, (int)tile.Location.Y, texture.Width, texture.Height);
                    area.Translate(viewport.X, viewport.Y);
                    
                    if (tile.SubSurfaceTextureId != null &&
                        TextureFactory.TryGetTexture(tile.SubSurfaceTextureId, out var subSurfaceTexture))
                    {
                        graphics.DrawImage(subSurfaceTexture.Image, area);
                    }
                    
                    if (tile.SurfaceTextureId != null &&
                        TextureFactory.TryGetTexture(tile.SurfaceTextureId, out var surfaceTexture))
                    { 
                        graphics.DrawImage(surfaceTexture.Image, area);
                    }
                    
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }
    }
}
