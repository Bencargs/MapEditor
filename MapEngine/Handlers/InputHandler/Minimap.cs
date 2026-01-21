using System;
using System.Numerics;
using Common;

namespace MapEngine.Handlers.InputHandler;

public class Minimap : IInterface
{
    private int _mapWidth;
    private int _mapHeight;
    private float _scale;
    
    // support letterboxed map area
    private int _offsetX;
    private int _offsetY;
    
    public Rectangle Area { get; private set;  }
    public IImage Background { get; private set; }
    
    public Vector2? ScreenToWorld(Vector2 point)
    {
        var x = point.X - Area.X;
        var y = point.Y - Area.Y;

        if (x < 0 || y < 0 || x >= Area.Width || y >= Area.Height)
            return null;

        x -= _offsetX;
        y -= _offsetY;
        if (x < 0 || y < 0)
            return null;
        
        var worldX = x / _scale;
        var worldY = y / _scale;
        if (worldX < 0 || worldY < 0 || worldX >= _mapWidth || worldY >= _mapHeight)
            return null;

        return new Vector2(worldX, worldY);
    }

    public Vector2 WorldToScreen(Vector2 point)
    {
        int x = _offsetX + (int)Math.Round(point.X * _scale);
        int y = _offsetY + (int)Math.Round(point.Y * _scale);

        return new Vector2(x, y);
    }

    public void Initialise(
        IImage background,
        Rectangle area,
        int mapWidth,
        int mapHeight,
        int offsetX,
        int offsetY,
        float scale)

    {
        Area = area;
        Background = background;

        _scale = scale;
        _offsetX = offsetX;
        _offsetY = offsetY;
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }
}