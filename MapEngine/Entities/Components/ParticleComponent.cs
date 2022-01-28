using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class ParticleComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Particle;
        public string ParticleType { get; set; }
        public string[] TextureIds { get; set; }
        public float? MinVelocity { get; set; }
        public float Lifetime { get; set; }
        public float SpawnRate { get; set; }
        public float FadeDelay { get; set; }
        public float FadeRate { get; set; }
        public float GrowRate { get; set; }
        public int MinInitialRotation { get; set; }
        public int MaxInitialRotation { get; set; }
        public int SpawnOffset { get; set; }
        public int? SpawnCount { get; set; }
        public float InitialSize { get; set; }
        public float InitialFade { get; set; }
        public int InitialVelocity { get; set; }
        public string PaletteTextureId { get; set; }
        public int PalleteSpeed { get; set; }

        public IComponent Clone()
        {
            return new ParticleComponent
            {
                TextureIds = TextureIds,
                ParticleType = ParticleType,
                SpawnRate = SpawnRate,
                MinVelocity = MinVelocity,
                Lifetime = Lifetime,
                FadeDelay = FadeDelay,
                FadeRate = FadeRate,
                GrowRate = GrowRate,
                MinInitialRotation = MinInitialRotation,
                MaxInitialRotation = MaxInitialRotation,
                SpawnOffset = SpawnOffset,
                SpawnCount = SpawnCount,
                InitialSize = InitialSize,
                InitialFade = InitialFade,
                InitialVelocity = InitialVelocity,
                PaletteTextureId = PaletteTextureId,
                PalleteSpeed = PalleteSpeed
            };
        }
    }
}
