using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Services.Effect;
using System.Collections.Generic;

namespace MapEngine.Handlers
{
    // todo: too broard? should there be a seperate handler for each effect type?
    public class EffectsHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
        , IHandleCommand<MoveCommand>
        , IHandleCommand<CreateEffectCommand>
    {
        private readonly FluidEffectService _fluidEffectService;
        private readonly List<Entity> _entities = new List<Entity>();

        public EffectsHandler(
            FluidEffectService fluidEffectService)
        {
            _fluidEffectService = fluidEffectService;
        }

        public void Initialise()
        {
            _fluidEffectService.Initialise();
        }

        public void Update()
        {
            // todo: consider dynamic obstructions, eg units moving
            //foreach (var e in _entities)
            //{
            //_fluidEffectService.SetSurface[previous, Fluid]
            //_fluidEffectService.SetSurface[current, Solid]
            //}
            _fluidEffectService.Simulate(1 / 60f, 5);
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            var fluidRender = _fluidEffectService.GenerateBitmap();
            graphics.DrawBytes(fluidRender, viewport);
        }

        public void Handle(CreateEntityCommand command)
        {
        }

        public void Handle(DestroyEntityCommand command)
        {
        }

        public void Handle(MoveCommand command)
        {
            // todo: update effect simulations considering entity interactions
        }

        public void Handle(CreateEffectCommand command)
        {
            switch (command.Name)
            {
                case "FluidEffect":
                    _fluidEffectService.SetEmitter(command.Location.X, command.Location.Y, command.Value);
                    break;
            }
        }
    }
}
