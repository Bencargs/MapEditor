using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapEngine.Entities.Components;
using Newtonsoft.Json;

namespace MapEngine.ResourceLoading
{
    public static class ParticleLoader
    {
        public static ParticleComponent LoadParticleDefinition(string filename)
        {
            var json = File.ReadAllText(filename);
            // Todo: this is a security risk
            dynamic particleData = JsonConvert.DeserializeObject(json);

            var textureIds = ToStringArray(particleData.TextureIds);

            var particle = new ParticleComponent
            {
                ParticleType = particleData.Type,
                TextureIds = textureIds,
                SpawnRate = particleData.SpawnRate,
                MinVelocity = particleData.MinVelocity,
                Lifetime = particleData.Lifetime,
                FadeDelay = particleData?.FadeDelay ?? 0,
                FadeRate = particleData.FadeRate,
                MinInitialRotation = particleData.MinInitialRotation,
                MaxInitialRotation = particleData.MaxInitialRotation,
                GrowRate = particleData.GrowRate,
                SpawnOffset = particleData?.SpawnOffset ?? 0,
                SpawnCount = particleData.SpawnCount,
                InitialSize = particleData.InitialSize,
                PaletteTextureId = particleData.PaletteTextureId
            };

            return particle;
        }

        private static string[] ToStringArray(dynamic property)
        {
            return ((IEnumerable<dynamic>) property).Select(x => (string) x).ToArray();
        }
    }
}
