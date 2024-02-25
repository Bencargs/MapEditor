using Common;
using MapEngine.Commands;
using MapEngine.Services.Effect;
using MapEngine.Services.Effects.WaveEffect;
using MapEngine.Services.Effects.LightingEffect;

namespace MapEngine.Handlers
{
    public class EffectsHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
        , IHandleCommand<MoveCommand>
        , IHandleCommand<CreateEffectCommand>
    {
        private readonly SensorHandler.SensorHandler _sensorHandler;
        private readonly WaveEffectService _waveService;
        private readonly FluidEffectService _fluidEffectService;
        private readonly LightingEffectService _lightingEffectService;
        private readonly ShadowEffectService _shadowService;

        public EffectsHandler(
            WaveEffectService waveService,
            FluidEffectService fluidEffectService, 
            LightingEffectService lightingEffectService, 
            SensorHandler.SensorHandler sensorHandler, 
            ShadowEffectService shadowService)
        {
            _waveService = waveService;
            _fluidEffectService = fluidEffectService;
            _lightingEffectService = lightingEffectService;
            _sensorHandler = sensorHandler;
            _shadowService = shadowService;
        }

        public void Initialise()
        {
            _waveService.Initialise();
            _fluidEffectService.Initialise();
            _shadowService.Initialise();
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

            var lightingRender = _lightingEffectService.GenerateBitmap(viewport);
            graphics.DrawBytes(lightingRender, viewport);

            var shadowRender = _shadowService.GenerateBitmap(viewport);
            graphics.DrawBytes(shadowRender, viewport);

            var sensorRender = _sensorHandler.GenerateBitmap(viewport, graphics);
            graphics.Desaturate(sensorRender, viewport);
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
