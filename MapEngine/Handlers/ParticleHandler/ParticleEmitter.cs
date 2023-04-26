using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;

namespace MapEngine.Handlers.ParticleHandler
{
    public class ParticleEmitter
    {
        public Entity Entity { get; }
        public bool IsComplete { get; private set; }
        private DateTime _previousSpawn = DateTime.Now;
        private readonly List<Particle> _particles = new List<Particle>();

        public ParticleEmitter(Entity entity)
        {
            Entity = entity;
        }

        public void Update()
        {
            var locationComponent = Entity.GetComponent<LocationComponent>();
            var movementComponent = Entity.GetComponent<MovementComponent>();
            var particleComponent = Entity.GetComponent<ParticleComponent>();

            // todo: make particle spawn conditions a generic method
            // Check spawn conditions
            var elapsed = (DateTime.Now - _previousSpawn).TotalMilliseconds;

            // todo: common random class - repeatable seed
            var rng = new Random();

            // Check initial conditions to spawn new particles
            if (ShouldEmit(elapsed, movementComponent, particleComponent))
            {
                // Spawn seed population
                while (particleComponent.InitialSpawnCount-- > 0)
                {
                    _previousSpawn = DateTime.Now;
                    var velocity = GetVelocity(rng, particleComponent);
                    var rotation = GetRotation(rng, locationComponent, particleComponent);
                    var location = GetLocation(rng, locationComponent, particleComponent);
                    var textureId = particleComponent.TextureIds[rng.Next(particleComponent.TextureIds.Length)];

                    _particles.Add(new Particle
                    {
                        Location = location,
                        Velocity = velocity,
                        Lifetime = 0,
                        Fade = particleComponent.InitialFade,
                        Size = particleComponent.InitialSize,
                        TextureId = textureId,
                        FacingAngle = rotation,
                        PaletteTextureId = particleComponent.PaletteTextureId,
                        PalleteSpeed = particleComponent.PalleteSpeed,
                    });
                }
            }

            // Age particles
            foreach (var p in _particles)
            {
                p.Lifetime += (float)elapsed;
                p.Size *= particleComponent.GrowRate;
                p.Location += p.Velocity;

                if (p.Lifetime > particleComponent.FadeDelay)
                    p.Fade = (byte) Math.Min(255, p.Fade + particleComponent.FadeRate); // todo: this should be timebased
            }

            // Delete expired particles
            _particles.RemoveAll(x => x.Lifetime > particleComponent.Lifetime ||
                                      x.Fade == 255);

            // should this logic be custom to each emitter type?
            IsComplete = !particleComponent.ContinousSpawn && _particles.Count == 0;
        }   

        // todo: particle renderer
        public void Draw(Rectangle viewport, IGraphics graphics)
        {
            // todo: order by age & Z value
            foreach (var p in _particles)
            {
                if (!viewport.Contains(p.Location))
                    continue;

                if (!TextureFactory.TryGetTexture(p.TextureId, out var texture))
                    continue;

                // rotate image to facing angle
                var rotated = texture.Image
                    .Scale(p.Size)
                    .Rotate(p.FacingAngle)
                    .Fade(p.Fade);

                if (TextureFactory.TryGetTexture(p.PaletteTextureId, out var palette))
                {
                    // todo: change this to stay on the last colour?
                    var hue = palette.Image.GetPalette(p.HueIndex++, p.PalleteSpeed);
                    rotated.ChangeHue(hue);
                }

                // Translate against camera movement
                var area = rotated.Area(p.Location);
                area.Translate(viewport.X, viewport.Y);

                graphics.DrawImage(rotated, area);
            }
        }

        private static Vector2 GetVelocity(Random rng, ParticleComponent particleComponent)
        {
            var jitterX = rng.Next(-particleComponent.InitialVelocity, particleComponent.InitialVelocity);
            var jitterY = rng.Next(-particleComponent.InitialVelocity, particleComponent.InitialVelocity);
            return new Vector2(jitterX, jitterY);
        }

        private static Vector2 GetLocation(Random rng, LocationComponent locationComponent, ParticleComponent particleComponent)
        {
            var jitterX = rng.Next(-particleComponent.SpawnOffset, particleComponent.SpawnOffset);
            var jitterY = rng.Next(-particleComponent.SpawnOffset, particleComponent.SpawnOffset);
            return new Vector2(locationComponent.Location.X + jitterX, locationComponent.Location.Y + jitterY);
        }

        private static float GetRotation(Random rng, LocationComponent locationComponent, ParticleComponent particleComponent)
        {
            var jitter = rng.Next(particleComponent.MinInitialRotation, particleComponent.MaxInitialRotation);
            return locationComponent.FacingAngle + jitter;
        }

        private static bool ShouldEmit(
            double elapsed,
            MovementComponent movementComponent,
            ParticleComponent particleComponent)
        {
            if (particleComponent.MinVelocity != null && (movementComponent.Velocity.Length() <= particleComponent.MinVelocity))
                return false;

            if (particleComponent.SpawnRate != null && (elapsed < particleComponent.SpawnRate))
                return false;

            if (particleComponent.TotalCount != null && particleComponent.TotalCount < 1)
                return false;

            if (particleComponent.ContinousSpawn)
                particleComponent.InitialSpawnCount = 1;

            return true;
        }
    }
}
