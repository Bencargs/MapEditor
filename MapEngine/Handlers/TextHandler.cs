using System;
using System.Windows;
using System.Windows.Media;
using Common;
using MapEngine.Commands;

namespace MapEngine.Handlers;

public class TextHandler : IHandleCommand<TextCommand>
{
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
                var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, 1.0);
                if (geometry == null || geometry.IsEmpty())
                {
                    startX += glyphWidth;
                    continue;
                }

                var offsetY = (int)(fontSize * glyphTypeface.Baseline);

                for (int py = 0; py < glyphHeight; py++)
                {
                    for (int px = 0; px < glyphWidth; px++)
                    {
                        var screenX = startX + px;
                        var screenY = startY + py;

                        if (screenX < 0 || screenX >= width || screenY < 0 || screenY >= height)
                            continue;

                        // Check if point is inside glyph geometry
                        var testPoint = new Point(px, py - offsetY);

                        if (geometry.FillContains(testPoint))
                        {
                            var pixelIndex = (screenY * stride) + (screenX * bytesPerPixel);
                            buffer[pixelIndex] = colour.Red;
                            buffer[pixelIndex + 1] = colour.Green;
                            buffer[pixelIndex + 2] = colour.Blue;
                            buffer[pixelIndex + 3] = colour.Alpha;
                        }
                    }
                }

                startX += glyphWidth;
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