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
                FadeRate = particleData?.FadeRate ?? 0,
                MinInitialRotation = particleData.MinInitialRotation,
                MaxInitialRotation = particleData.MaxInitialRotation,
                GrowRate = particleData?.GrowRate ?? 1,
                SpawnOffset = particleData?.SpawnOffset ?? 0,
                TotalCount = particleData.TotalCount,
                InitialSpawnCount = particleData?.InitialSpawnCount ?? 1,
                InitialSize = particleData.InitialSize,
                InitialFade = particleData?.InitialFade ?? 0,
                InitialVelocity = particleData?.InitialVelocity ?? 0,
                PaletteTextureId = particleData.PaletteTextureId,
                PalleteSpeed = particleData?.PalleteSpeed ?? 1,
                ContinousSpawn = particleData?.ContinousSpawn ?? false
            };

            return particle;
        }

        private static string[] ToStringArray(dynamic property)
        {
            return ((IEnumerable<dynamic>) property).Select(x => (string) x).ToArray();
        }
    }
}
