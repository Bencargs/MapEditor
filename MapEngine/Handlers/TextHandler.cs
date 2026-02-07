using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Common;
using MapEngine.Commands;

namespace MapEngine.Handlers;

public class TextHandler : IHandleCommand<TextCommand>
{
    private static readonly Dictionary<GlyphCacheKey, byte[]> _glyphCache = new();
    
    public void DrawText(
            byte[] buffer,
            string text,
            Rectangle area,
            Font font,
            double fontSize,
            Colour colour,
            Justification justification = Justification.Left)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        
        var shadowColour = new Colour(0, 0, 0);
        var glyphTypeface = font.GlyphTypeface;
        
        var width = area.Width;
        var height = area.Height;
        var bytesPerPixel = 4;
        var stride = width * bytesPerPixel;

        var startY = area.Y;
        var startX = (int)(justification switch
        {
            Justification.Left => area.X,
            Justification.Center => area.X - TextWidth(text, fontSize, glyphTypeface) / 2f,
            Justification.Right => area.X - TextWidth(text, fontSize, glyphTypeface),
        });

        // Shadow
        RenderText(buffer, text, fontSize, shadowColour, glyphTypeface, startX + 1, startY + 1, width, height, stride, bytesPerPixel);
        
        // Text
        RenderText(buffer, text, fontSize, colour, glyphTypeface, startX, startY, width, height, stride, bytesPerPixel);
    }

    private static void RenderText(
        byte[] buffer, 
        string text, 
        double fontSize, 
        Colour colour, 
        GlyphTypeface glyphTypeface,
        int startX, int startY, int width, int height, 
        int stride, int bytesPerPixel)
    {
        foreach (var ch in text)
        {
            if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(ch, out var glyphIndex))
                continue;

            var advanceWidth = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
            var glyphWidth = (int)Math.Ceiling(advanceWidth);
            var glyphHeight = (int)Math.Ceiling(fontSize);
            
            // Get or create cached glyph bitmap
            var cacheKey = new GlyphCacheKey(glyphTypeface, glyphIndex, fontSize);
            if (!_glyphCache.TryGetValue(cacheKey, out var glyphBitmap))
            {
                glyphBitmap = RasterizeGlyph(glyphTypeface, glyphIndex, fontSize, glyphWidth, glyphHeight);
                _glyphCache[cacheKey] = glyphBitmap;
            }

            // Fast blit from cached glyph
            var offsetY = (int)(fontSize * glyphTypeface.Baseline);
            BlitGlyph(buffer, glyphBitmap, glyphWidth, glyphHeight, startX, startY, offsetY, 
                     width, height, stride, bytesPerPixel, colour);

            startX += glyphWidth;
        }
    }

    private static byte[] RasterizeGlyph(GlyphTypeface glyphTypeface, ushort glyphIndex, 
                                        double fontSize, int glyphWidth, int glyphHeight)
    {
        var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, 1.0);
        var offsetY = (int)(fontSize * glyphTypeface.Baseline);
        
        // Create a bitmap to hold the glyph mask
        var glyphBitmap = new byte[glyphWidth * glyphHeight];
        
        if (geometry == null || geometry.IsEmpty())
            return glyphBitmap;

        // Rasterize geometry once
        for (int py = 0; py < glyphHeight; py++)
        {
            for (int px = 0; px < glyphWidth; px++)
            {
                var testPoint = new Point(px, py - offsetY);
                if (geometry.FillContains(testPoint))
                {
                    glyphBitmap[py * glyphWidth + px] = 255; // Mark as filled
                }
            }
        }

        return glyphBitmap;
    }

    private static void BlitGlyph(byte[] buffer, byte[] glyphBitmap, int glyphWidth, int glyphHeight,
                                 int startX, int startY, int offsetY, int width, int height, 
                                 int stride, int bytesPerPixel, Colour colour)
    {
        for (int py = 0; py < glyphHeight; py++)
        {
            var screenY = startY + py;
            if (screenY < 0 || screenY >= height)
                continue;

            var glyphRowStart = py * glyphWidth;
            var screenRowStart = screenY * stride;

            for (int px = 0; px < glyphWidth; px++)
            {
                if (glyphBitmap[glyphRowStart + px] == 0)
                    continue;

                var screenX = startX + px;
                if (screenX < 0 || screenX >= width)
                    continue;

                var pixelIndex = screenRowStart + (screenX * bytesPerPixel);
                buffer[pixelIndex] = colour.Red;
                buffer[pixelIndex + 1] = colour.Green;
                buffer[pixelIndex + 2] = colour.Blue;
                buffer[pixelIndex + 3] = colour.Alpha;
            }
        }
    }

    public void Handle(TextCommand command)
    {
        // todo: populate chat log (timestamp, player, text), process input for commands
    }

    private static double TextWidth(string text, double size, GlyphTypeface glyphTypeface)
    {
        var totalWidth = 0.0;
        foreach (var ch in text)
        {
            if (glyphTypeface.CharacterToGlyphMap.TryGetValue(ch, out var glyphIndex))
            {
                totalWidth += glyphTypeface.AdvanceWidths[glyphIndex] * size;
            }
        }

        return totalWidth;
    }
}

public enum Justification
{
    Left, 
    Center, 
    Right
}

internal readonly struct GlyphCacheKey : IEquatable<GlyphCacheKey>
{
    private readonly GlyphTypeface _typeface;
    private readonly ushort _glyphIndex;
    private readonly double _fontSize;

    public GlyphCacheKey(GlyphTypeface typeface, ushort glyphIndex, double fontSize)
    {
        _typeface = typeface;
        _glyphIndex = glyphIndex;
        _fontSize = fontSize;
    }

    public bool Equals(GlyphCacheKey other)
    {
        return _typeface == other._typeface && 
               _glyphIndex == other._glyphIndex && 
               Math.Abs(_fontSize - other._fontSize) < 0.01;
    }

    public override bool Equals(object obj) => obj is GlyphCacheKey other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = _typeface?.GetHashCode() ?? 0;
            hash = (hash * 397) ^ _glyphIndex.GetHashCode();
            hash = (hash * 397) ^ _fontSize.GetHashCode();
            return hash;
        }
    }
}