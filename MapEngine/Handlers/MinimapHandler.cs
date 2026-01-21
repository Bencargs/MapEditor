using System;
using System.Collections.Generic;
using System.Numerics;
using Common;
using Common.Collision;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.Handlers.InputHandler;
using MapEngine.Services.Map;

namespace MapEngine.Handlers;

public class MinimapHandler : 
    IHandleCommand<CreateEntityCommand>,
    IHandleCommand<DestroyEntityCommand>
{
    private readonly MapService _map;
    private readonly Minimap _minimap;
    private readonly List<Entity> _entities = new();

    //todo: common UI theme colour, via config
    //todo: 'friendlyColor' to property of entity unit component
    private readonly Colour _friendlyColour = new Colour(50, 50, 240);
    private readonly Colour _boarderColour = new Colour(50, 50, 240, 60);
    private readonly Colour _interfaceColour = new Colour(215, 215, 230);
    
    public MinimapHandler(MapService map, Minimap minimap)
    {
        _map = map;
        _minimap = minimap;
    }

    public void Initialise()
    {
        // todo: via interface config
        var area = new Rectangle(0, 0, 300, 200);
        
        var scaleX =  (float) area.Width / _map.Width;
        var scaleY =  (float) area.Height / _map.Height;
        var scale = Math.Min(scaleX, scaleY);
        var scaledMapWidth = (int)Math.Round(_map.Width * scale);
        var scaledMapHeight = (int)Math.Round(_map.Height * scale);
        var offsetX = (area.Width - scaledMapWidth) / 2;
        var offsetY = (area.Height - scaledMapHeight) / 2;
        
        var background = BuildBackground(_map.TextureTiles, area, offsetX, offsetY, scale);
        
        _minimap.Initialise(background, area, _map.Width, _map.Height, offsetX, offsetY, scale);
    }

    public void Render(Rectangle viewport, byte[] buffer)
    {
        DrawBackground(buffer);
        DrawEntities(buffer);
        DrawViewport(buffer, viewport);
        DrawRectangle(buffer, _map.Width, _map.Height,
            0, 0, _minimap.Area.Width, _minimap.Area.Height,
            _interfaceColour); //todo: common UI theme colour
    }

    private void DrawBackground(byte[] buffer)
    {
        for (int x = 0; x < _minimap.Area.Width; x++)
        {
            for (int y = 0; y < _minimap.Area.Height; y++)
            {
                var colour = _minimap.Background[x, y];
                var index = (x * 4) + (y * _map.Width * 4);
                buffer[index + 0] = colour.Red;
                buffer[index + 1] = colour.Blue;
                buffer[index + 2] = colour.Green;
                buffer[index + 3] = 255;
            }
        }
    }

    private void DrawEntities(byte[] buffer)
    {
        foreach (var entity in _entities)
        {
            var locationComponent = entity.GetComponent<LocationComponent>();
            var collisionComponent = entity.GetComponent<CollisionComponent>();

            if (locationComponent == null || collisionComponent == null)
                continue;
            
            var mapLocation = _minimap.WorldToScreen(locationComponent.Location);
            var x = (int)mapLocation.X;
            var y = (int)mapLocation.Y;
            const int radius = 2;
            
            DrawFilledCircle(buffer, _map.Width, _map.Height, x, y, radius+1, _friendlyColour);
            DrawFilledCircle(buffer, _map.Width, _map.Height, x, y, radius, _boarderColour);
        }
    }
    
    private void DrawFilledCircle(byte[] buffer, int screenWidth, int screenHeight, int cx, int cy, int radius, Colour colour)
    {
        int r2 = radius * radius;

        // clamp to minimap area to avoid painting outside UI
        int minX = Math.Max(_minimap.Area.X, cx - radius);
        int maxX = Math.Min(_minimap.Area.X + _minimap.Area.Width - 1, cx + radius);
        int minY = Math.Max(_minimap.Area.Y, cy - radius);
        int maxY = Math.Min(_minimap.Area.Y + _minimap.Area.Height - 1, cy + radius);

        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - cy;
            int dy2 = dy * dy;

            // For this y, solve x extent: dx^2 + dy^2 <= r^2
            int dxMax = (int)Math.Floor(Math.Sqrt(Math.Max(0, r2 - dy2)));

            int startX = Math.Max(minX, cx - dxMax);
            int endX   = Math.Min(maxX, cx + dxMax);

            for (int x = startX; x <= endX; x++)
            {
                // If you want alpha blending, swap SetPixel for BlendPixel below.
                SetPixel(buffer, screenWidth, screenHeight, x, y, colour);
            }
        }
    }

    private void DrawViewport(byte[] buffer, Rectangle viewport)
    {
        // Top-left and bottom-right of the world viewport -> minimap space
        var topLeft = _minimap.WorldToScreen(new System.Numerics.Vector2(viewport.X, viewport.Y));
        var bottomRight = _minimap.WorldToScreen(new System.Numerics.Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height));

        int x = (int)Math.Min(topLeft.X, bottomRight.X);
        int y = (int)Math.Min(topLeft.Y, bottomRight.Y);
        int w = Math.Max(1, (int)Math.Abs(bottomRight.X - topLeft.X));
        int h = Math.Max(1, (int)Math.Abs(bottomRight.Y - topLeft.Y));

        // Clamp to minimap area so it doesn’t draw into the rest of the screen buffer
        int minX = _minimap.Area.X;
        int minY = _minimap.Area.Y;
        int maxX = _minimap.Area.X + _minimap.Area.Width;
        int maxY = _minimap.Area.Y + _minimap.Area.Height;

        if (x < minX) { w -= (minX - x); x = minX; }
        if (y < minY) { h -= (minY - y); y = minY; }
        if (x + w > maxX) w = maxX - x;
        if (y + h > maxY) h = maxY - y;

        if (w <= 0 || h <= 0) return;

        // Draw viewport outline (white-ish)
        DrawRectangle(buffer, _map.Width, _map.Height, x+1, y, w-2, h-1, _friendlyColour);
    }

    // todo: surely stop repeating this and make a common method
    private static void DrawRectangle(byte[] buffer, int mapWidth, int mapHeight, int x, int y, int minimapWidth, int minimapHeight, Colour colour)
    {
        // top + bottom
        for (int i = 0; i < minimapWidth; i++)
        {
            SetPixel(buffer, mapWidth, mapHeight, x + i, y, colour);
            SetPixel(buffer, mapWidth, mapHeight, x + i, y + minimapHeight - 1, colour);
        }
        // left + right
        for (int j = 0; j < minimapHeight; j++)
        {
            SetPixel(buffer, mapWidth, mapHeight, x, y + j, colour);
            SetPixel(buffer, mapWidth, mapHeight, x + minimapWidth - 1, y + j, colour);
        }
    }
    
    private static void SetPixel(byte[] buffer, int w, int h, int x, int y, Colour c)
    {
        if ((uint)x >= (uint)w || (uint)y >= (uint)h) return;

        int idx = (y * w + x) * 4;
        buffer[idx + 0] = c.Blue;
        buffer[idx + 1] = c.Green;
        buffer[idx + 2] = c.Red;
        buffer[idx + 3] = c.Alpha;
    }

    private static IImage BuildBackground(Tile[,] tiles, Rectangle area, int offsetX, int offsetY, float scale)
    {
        var background = new WpfImage(area.Width, area.Height);
        foreach (var tile in tiles)
        {
            if (TextureFactory.TryGetTexture(tile.SubSurfaceTextureId, out var subSurfaceTexture))
            {
                DrawImage(subSurfaceTexture, background, area, offsetX, offsetY, scale);
            }
            
            if (TextureFactory.TryGetTexture(tile.TextureId, out var surfaceTexture))
            {
                DrawImage(surfaceTexture, background, area, offsetX, offsetY, scale);
            }
        }

        return background;
    }

    private static void DrawImage(Texture surfaceTexture, WpfImage background, Rectangle area, int offsetX, int offsetY, float scale)
    {
        var source = surfaceTexture.Image;
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        
        var invScale = 1f / scale;
        var sourceStride = source.Width * 4;
        var destStride = area.Width * 4;
    
        for (int dy = 0; dy < height; dy++)
        {
            int destY = offsetY + dy;
            if (destY < 0 || destY >= area.Height)
                continue;
        
            int sourceY = Math.Max(0, Math.Min(source.Height - 1, (int)(dy * invScale)));
            int sourceRowOffset = sourceY * sourceStride;
            int destRowOffset = destY * destStride;
        
            for (int dx = 0; dx < width; dx++)
            {
                int destX = offsetX + dx;
                if (destX < 0 || destX >= area.Width)
                    continue;
            
                int sourceX = Math.Max(0, Math.Min(source.Width - 1, (int)(dx * invScale)));
                var sourceIndex = sourceRowOffset + sourceX * 4;
                var destIndex = destRowOffset + destX * 4;
                
                if (source.Buffer[sourceIndex + 3] == 0)
                    continue;
                
                background.Buffer[destIndex + 0] = source.Buffer[sourceIndex + 0];
                background.Buffer[destIndex + 1] = source.Buffer[sourceIndex + 1];
                background.Buffer[destIndex + 2] = source.Buffer[sourceIndex + 2];
                background.Buffer[destIndex + 3] = source.Buffer[sourceIndex + 3];
            }
        }
    }
    
    public void Handle(CreateEntityCommand command)
    {
        var entity = command.Entity;
        if (entity.GetComponent<LocationComponent>() == null)
            return;
        
        _entities.Add(entity);
    }

    public void Handle(DestroyEntityCommand command)
    {
        _entities.Remove(command.Entity);
    }
}