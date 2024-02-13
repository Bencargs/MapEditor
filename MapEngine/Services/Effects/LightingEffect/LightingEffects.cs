using Common;

namespace MapEngine.Services.Effects.LightingEffect
{
    public class LightingEffects
    {
        public AmbientLight[] Ambient { get; set; }
        public DiffuseLight[] Diffuse { get; set; }

        public class AmbientLight
        {
            public string Name { get; set; }
            // todo: refactor this, really just need a colour and a radius
            public string TextureId { get; set; }
            public int On { get; set; }
            public int Off { get; set; }
        }

        public class DiffuseLight
        {
            public string Name { get; set; }
            public Colour Colour { get; set; }
            public bool IsTransition { get; set; }
            public int On { get; set; }
            public int Off { get; set; }
        }
    }
}
