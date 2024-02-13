using Common;
using MapEngine.Services.Effects.FluidEffect;
using MapEngine.Services.Effects.LightingEffect;
using MapEngine.Services.Effects.WaveEffect;

namespace MapEngine.Services.Map
{
    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Team[] Teams { get; set; }
        public Tile[,] Tiles { get; set; }
        public FluidEffects FluidEffects { get; set; }
        public WaveEffects WaveEffects { get; set; }
        public LightingEffects LightingEffects { get; set; }
    }
}
