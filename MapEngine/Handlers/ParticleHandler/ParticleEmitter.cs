using System;
using System.Collections.Generic;
using Common;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;

namespace MapEngine.Handlers.ParticleHandler
{
    public class ParticleEmitter
    {
        public Entity Entity { get; }
        private DateTime _previousSpawn;
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
            if (movementComponent.Velocity.Length() > particleComponent.MinVelocity)
            {
                if (elapsed > particleComponent.SpawnRate)
                {
                    // todo: for some particle emitters some values should be ransomised, eg direciton, velocity, location
                    // todo: common random class - repeatable seed
                    var rng = new Random();
                    
                    // Spawn new particles
                    _previousSpawn = DateTime.Now;
                    var rotation = locationComponent.FacingAngle + rng.Next(particleComponent.MinInitialRotation, particleComponent.MaxInitialRotation);
                    var textureId = particleComponent.TextureIds[rng.Next(particleComponent.TextureIds.Length)];
                    
                    _particles.Add(new Particle
                    {
                        Location = locationComponent.Location,
                        Lifetime = 0,
                        Fade = 0,
                        Size = 1,
                        TextureId = textureId,
                        FacingAngle = rotation,
                    });
                }
            }

            // Age particles
            foreach (var p in _particles)
            {
                p.Lifetime += (float)elapsed;
                p.Size *= particleComponent.GrowRate;
                p.Fade = (byte) Math.Min(255, p.Fade + particleComponent.FadeRate); // todo: this should be timebased
            }

            // Delete expired particles
            _particles.RemoveAll(x => x.Lifetime > particleComponent.Lifetime ||
                                      x.Fade == 255);
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
                    .Rotate(p.FacingAngle)
                    .Scale(p.Size)
                    .Fade(p.Fade);

                // Translate against camera movement
                var area = rotated.Area(p.Location);
                area.Translate(viewport.X, viewport.Y);

                graphics.DrawImage(rotated, area);
            }
        }
    }
}
