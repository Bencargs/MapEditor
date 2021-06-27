using System.Collections.Generic;
using Common;
using MapEngine.Commands;
using MapEngine.Entities.Components;

namespace MapEngine.Handlers.ParticleHandler
{
    public class ParticleHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly List<ParticleEmitter> _emitters = new List<ParticleEmitter>();
        
        public void Update()
        {
            foreach (var e in _emitters)
            {
                e.Update();
            }
        }

        // todo: this class is pretty anemic - whats the point?
        public void Render(Rectangle viewport, IGraphics graphics)
        {
            foreach (var e in _emitters)
            {
                e.Draw(viewport, graphics);
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (entity.GetComponent<ParticleComponent>() == null)
                return;

            var emitter = new ParticleEmitter(entity);
            _emitters.Add(emitter);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _emitters.RemoveAll(x => x.Entity.Id == command.Entity.Id);
        }
    }
}
