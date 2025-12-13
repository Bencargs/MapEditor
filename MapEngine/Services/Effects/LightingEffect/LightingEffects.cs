using System.Numerics;
using Common;

namespace MapEngine.Services.Effects.LightingEffect
{
    public class LightingEffects
    {
        public AmbientLight[] Ambient { get; set; }
        public DiffuseLight[] Diffuse { get; set; }

        public class AmbientLight
        {
            // todo: refactor this, really just need a colour and a radius
            public string TextureId { get; set; }
            public bool LineOfSight { get; set; }
            public int On { get; set; }
            public int Off { get; set; }
            public Vector2 Location { get; set; }
        }

        public class DiffuseLight
        {
            public string Name { get; set; }
            public Colour Colour { get; set; }
            public TransitionType TransitionType { get; set; }
            public int On { get; set; }
            public int Off { get; set; }
        }

        public enum TransitionType
        {
            None = 0,
            Immediate,
            Fade,
        }
    }
}
