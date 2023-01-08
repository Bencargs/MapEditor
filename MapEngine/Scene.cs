using Common;
using MapEditor.Common;
using MapEngine.Commands;
using MapEngine.Handlers;
using System;
using System.Linq;

namespace MapEngine
{
    public class Scene
    {
        private readonly IGraphics _graphics;
        private readonly MessageHub _messageHub;
        private readonly MapHandler _mapHandler;
        private readonly EntityHandler _unitHandler;
        private readonly CameraHandler _cameraHandler;
        private readonly EffectsHandler _effectsHandler;

        public Scene(
            IGraphics graphics,
            MessageHub messageHub,
            MapHandler mapHandler,
            EntityHandler unitHandler,
            CameraHandler cameraHandler,
            EffectsHandler effectsHandler)
        {
            _graphics = graphics;
            _messageHub = messageHub;
            _mapHandler = mapHandler;
            _unitHandler = unitHandler;
            _cameraHandler = cameraHandler;
            _effectsHandler = effectsHandler;
        }

        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap7.json";
            _cameraHandler.Initialise(mapFilename);
            _mapHandler.Initialise(mapFilename);
            
            var weaponsPath = @"C:\Source\MapEditor\MapEngine\Content\Weapons\";
            var unitsPath = @"C:\Source\MapEditor\MapEngine\Content\Units\";
            var modelsPath = @"C:\Source\MapEditor\MapEngine\Content\Models";
            _unitHandler.Initialise(unitsPath, mapFilename, weaponsPath, modelsPath);

            _effectsHandler.Initialise();
            for (var i = 0; i < 512; i++)
            {
                // todo: temporary
                _messageHub.Post(new CreateEffectCommand
                {
                    Name = "FluidEffect", // todo: these should be constant or enum values?
                    Location = new Vector2 { X = 1, Y = i },
                    Value = 2f
                });
            }
        }

        double _totalElapsed = 0;
        int _frameCount = 0;
        public void Display()
        {
            // todo: currently fixed timestep gameloop - upgrade to variable
            var startTime = DateTime.Now;

            Update();

            Render();

            var endTime = DateTime.Now;
            var elapsed = (endTime - startTime).TotalMilliseconds;
            _totalElapsed += elapsed;
            _frameCount++;

            Console.WriteLine($"Average FPS: {_frameCount / _totalElapsed * 1000f}");
        }

        private void Update()
        {
            _messageHub.Notify();
            _cameraHandler.Update();
            _unitHandler.Update();
            _effectsHandler.Update();
        }

        private void Render()
        {
            var viewport = _cameraHandler.GetViewport();
            _mapHandler.Render(viewport, _graphics);
            _unitHandler.Render(viewport, _graphics);
            _effectsHandler.Render(viewport, _graphics);

            _graphics.Render();
        }
    }
}
