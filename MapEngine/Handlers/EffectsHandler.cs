using Common;
using MapEngine.Commands;
using MapEngine.Services.Effects;

namespace MapEngine.Handlers
{
    public class EffectsHandler
        : IHandleCommand<CreateEffectCommand>
    {
        private readonly WaveEffectService _waveService;

        public EffectsHandler(MessageHub messageHub, WaveEffectService waveService)
        {
            _waveService = waveService;
        }

        public void Initialise()
        {
            _waveService.Initialise();
        }

        public void Update()
        {
            _waveService.CalculateForces();
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            var render = _waveService.GenerateBitmap();
            //graphics.DrawBytes(render, viewport);
            graphics.Apply(render);
        }

        public void Handle(CreateEffectCommand command)
        {
            _waveService.SetHeight(
                (int)command.Location.X,
                (int)command.Location.Y,
                command.Value);
        }
    }
}
