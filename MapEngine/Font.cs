using System.Windows.Media;

namespace MapEngine;

public record Font
{
    public string FontId { get; set; }
    public GlyphTypeface GlyphTypeface { get; set; }
}