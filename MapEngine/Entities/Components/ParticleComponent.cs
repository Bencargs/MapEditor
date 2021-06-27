using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class ParticleComponent : IComponent
    {
        public ComponentType Type { get; } = ComponentType.Particle;
        public string ParticleType { get; set; }
        public string[] TextureIds { get; set; }
        public float MinVelocity { get; set; }
        public float Lifetime { get; set; }
        public float SpawnRate { get; set; }
        public float FadeRate { get; set; }
        public float GrowRate { get; set; }
        public int MinInitialRotation { get; set; }
        public int MaxInitialRotation { get; set; }

        public IComponent Clone()
        {
            return new ParticleComponent
            {
                TextureIds = TextureIds,
                ParticleType = ParticleType,
                SpawnRate = SpawnRate,
                MinVelocity = MinVelocity,
                Lifetime = Lifetime,
                FadeRate = FadeRate,
                GrowRate = GrowRate,
                MinInitialRotation = MinInitialRotation,
                MaxInitialRotation = MaxInitialRotation,
            };
        }
    }
}
