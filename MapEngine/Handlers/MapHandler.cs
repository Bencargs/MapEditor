using System;
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
                // todo: fix
                // if (!viewport.Contains(tile.Location))
                //     continue;

                if (!TextureFactory.TryGetTexture(tile.TextureId, out var texture)) 
                    continue;
                
                var area = new Rectangle(
                    (int)(tile.Location.X - viewport.X),
                    (int)(tile.Location.Y - viewport.Y),
                    texture.Width,
                    texture.Height);
                    
                if (tile.SubSurfaceTextureId != null &&
                    TextureFactory.TryGetTexture(tile.SubSurfaceTextureId, out var subSurfaceTexture))
                {
                    DrawTiled(graphics, subSurfaceTexture, area);
                }
                    
                if (tile.SurfaceTextureId != null &&
                    TextureFactory.TryGetTexture(tile.SurfaceTextureId, out var surfaceTexture))
                {
                    DrawTiled(graphics, surfaceTexture, area);
                }
                    
                graphics.DrawImage(texture.Image, area);
            }
        }
        
        private void DrawTiled(IGraphics graphics, Texture texture, Rectangle dest)
        {
            var startX = dest.X;
            var startY = dest.Y;
            var endX = dest.Width;
            var endY = dest.Height;

            for (var x = startX; x < endX; x += texture.Image.Width)
            {
                var width = Math.Min(texture.Image.Width, endX - x);

                for (var y = startY; y < endY; y += texture.Image.Height)
                {
                    var height = Math.Min(texture.Image.Height, endY - y);

                    var area = new Rectangle(x, y, width, height);
                    graphics.DrawImage(texture.Image, area);
                }
            }
        }
    }
}
