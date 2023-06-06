using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapEngine.Entities.Components;
using MapEngine.ResourceLoading;

namespace MapEngine.Factories
{
    public class ParticleFactory
    {
        private static readonly Dictionary<string, ParticleComponent> _prototypes =
            new Dictionary<string, ParticleComponent>(StringComparer.OrdinalIgnoreCase);

        public static void LoadParticles(string particlesFilepath)
        {
            foreach (var file in Directory.GetFiles(particlesFilepath, "*.json"))
            {
                var particle = ParticleLoader.LoadParticleDefinition(file);
                _prototypes.Add(particle.ParticleType, particle);
            }
        }

        public static bool TryGetParticle(string type, out ParticleComponent particle)
        {
            if (!_prototypes.TryGetValue(type, out particle))
                return false;

            particle = (ParticleComponent) particle.Clone();
            return true;
        }
    }
}
