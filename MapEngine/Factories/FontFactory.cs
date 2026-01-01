using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace MapEngine.Factories;

public class FontFactory
{
    private static readonly Dictionary<string, Font> _fonts =
        new(StringComparer.OrdinalIgnoreCase);

    public static void LoadFonts(string folderPath)
    {
        foreach (var file in Directory.GetFiles(folderPath, "*.ttf"))
            LoadFontFile(file);
    }

    public static bool TryGetFont(string fontId, out Font font)
    {
        return _fonts.TryGetValue(fontId, out font);
    }

    private static void LoadFontFile(string filePath)
    {
        var key = Path.GetFileNameWithoutExtension(filePath);

        var uri = new Uri(filePath, UriKind.Absolute);
        var glyph = new GlyphTypeface(uri);
        
        _fonts[key] = new Font{ FontId = key, GlyphTypeface = glyph};
    }
}