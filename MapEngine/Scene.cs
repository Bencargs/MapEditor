using Common;
using System.Numerics;
using MapEngine.Commands;
using MapEngine.Handlers;
using System;
using System.Diagnostics;
using MapEngine.Handlers.ParticleHandler;

namespace MapEngine
{
    public class Scene
    {
        private readonly IGraphics _graphics;
        private readonly MessageHub _messageHub;
        private readonly GameTime _gameTime;
        private readonly MapHandler _mapHandler;
        private readonly EntityHandler _unitHandler;
        private readonly CameraHandler _cameraHandler;
        private readonly EffectsHandler _effectsHandler;
        private readonly ParticleHandler _particleHandler;
        private readonly InterfaceHandler _interfaceHandler;

        public Scene(
            IGraphics graphics,
            MessageHub messageHub,
            GameTime gameTime,
            MapHandler mapHandler,
            EntityHandler unitHandler,
            CameraHandler cameraHandler,
            EffectsHandler effectsHandler,
            ParticleHandler particleHandler,
            InterfaceHandler interfaceHandler)
        {
            _graphics = graphics;
            _messageHub = messageHub;
            _gameTime = gameTime;
            _mapHandler = mapHandler;
            _unitHandler = unitHandler;
            _cameraHandler = cameraHandler;
            _effectsHandler = effectsHandler;
            _particleHandler = particleHandler;
            _interfaceHandler = interfaceHandler;
        }

        public void Initialise()
        {
            var mapFilename = @"C:\src\MapEditor\MapEngine\Content\Maps\TestMap13.json";
            _cameraHandler.Initialise(mapFilename);
            _mapHandler.Initialise(mapFilename);
            
            var weaponsPath = @"C:\src\MapEditor\MapEngine\Content\Weapons\";
            var unitsPath = @"C:\src\MapEditor\MapEngine\Content\Units\";
            var modelsPath = @"C:\src\MapEditor\MapEngine\Content\Models";
            var particlesPath = @"C:\src\MapEditor\MapEngine\Content\Particles";
            _unitHandler.Initialise(unitsPath, mapFilename, weaponsPath, modelsPath, particlesPath);

            _effectsHandler.Initialise();
            _interfaceHandler.Initialise(@"C:\src\MapEditor\MapEngine\Content\Cursors");
        }

        public void Display()
        {
            // todo: move to gametime
            // todo: currently fixed timestep gameloop - upgrade to variable
            _gameTime.StartFrame();

            Update();

            Render();

            _gameTime.EndFrame();

            Debug.WriteLine($"Average FPS: {_gameTime.CalculateAverageFps()}");
        }

        private void Update()
        {
            _messageHub.Notify();
            _cameraHandler.Update();
            _particleHandler.Update();
            _unitHandler.Update();
            _effectsHandler.Update();
        }

        private void Render()
        {
            // Todo: need to order rendering by Z height
            var viewport = _cameraHandler.GetViewport();
            _mapHandler.Render(viewport, _graphics);
            _particleHandler.Render(viewport, _graphics);
            _unitHandler.Render(viewport, _graphics);
            _effectsHandler.Render(viewport, _graphics);
            _interfaceHandler.Render(viewport, _graphics);

            _graphics.Render();
        }
    }
}
