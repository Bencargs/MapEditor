using Common;
using MapEngine.Commands;
using MapEngine.Handlers;

namespace MapEngine
{
    public class Scene
    {
        private readonly IGraphics _graphics;
        private readonly MessageHub _messageHub;
        private readonly MapHandler _mapHandler;
        private readonly EntityHandler _unitHandler;
        private readonly CameraHandler _cameraHandler;

        public Scene(
            IGraphics graphics,
            MessageHub messageHub,
            MapHandler mapHandler,
            EntityHandler unitHandler,
            CameraHandler cameraHandler)
        {
            _graphics = graphics;
            _messageHub = messageHub;
            _mapHandler = mapHandler;
            _unitHandler = unitHandler;
            _cameraHandler = cameraHandler;
        }

        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap6.json";
            _cameraHandler.Initialise(mapFilename);
            _mapHandler.Initialise(mapFilename);

            var weaponsPath = @"C:\Source\MapEditor\MapEngine\Content\Weapons\";
            var unitsPath = @"C:\Source\MapEditor\MapEngine\Content\Units\";
            var modelsPath = @"C:\Source\MapEditor\MapEngine\Content\Models";
            _unitHandler.Initialise(unitsPath, mapFilename, weaponsPath, modelsPath);
        }

        public void Display()
        {
            Update();

            Render();
        }

        private void Update()
        {
            _messageHub.Notify();
            _cameraHandler.Update();
            _unitHandler.Update();
        }

        private void Render()
        {
            _graphics.Clear();

            var viewport = _cameraHandler.GetViewport();
            _mapHandler.Render(viewport, _graphics);
            _unitHandler.Render(viewport, _graphics);

            _graphics.Render();
        }
    }
}
