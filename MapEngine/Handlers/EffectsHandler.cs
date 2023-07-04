using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Services.Effect;
using MapEngine.Services.Effects.WaveEffect;
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
        private readonly WaveEffectService _waveService;
        private readonly FluidEffectService _fluidEffectService;
        private readonly List<Entity> _entities = new List<Entity>();

        public EffectsHandler(
            WaveEffectService waveService,
            FluidEffectService fluidEffectService)
        {
            _waveService = waveService;
            _fluidEffectService = fluidEffectService;
        }

        public void Initialise()
        {
            _waveService.Initialise();
            _fluidEffectService.Initialise();
        }

        public void Update()
        {
            _waveService.Simulate();

            _fluidEffectService.Simulate(1 / 60f, 5);
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            var waveRender = _waveService.GenerateBitmap();
            graphics.DrawBytes(waveRender, viewport);

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
                case "WaveEffect":
                    _waveService.SetHeight(
                        (int)command.Location.X,
                        (int)command.Location.Y,
                        command.Value);
                    break;

                case "FluidEffect":
                    _fluidEffectService.SetEmitter(
                        (int)command.Location.X,
                        (int)command.Location.Y, 
                        command.Value);
                    break;
            }
        }
    }
}
