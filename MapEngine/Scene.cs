using Common;
using MapEngine.Handlers;

namespace MapEngine
{
    public class Scene
    {
        private readonly IGraphics _graphics;
        private readonly UnitHandler _unitHandler;
        private readonly MapHandler _mapHandler;
        private readonly CameraHandler _cameraHandler;

        public Scene(IGraphics graphics, CameraHandler cameraHandler, MapHandler mapHandler, UnitHandler unitHandler)
        {
            _graphics = graphics;
            _cameraHandler = cameraHandler;
            _mapHandler = mapHandler;
            _unitHandler = unitHandler;
        }

        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap0.json";
            _cameraHandler.Init(mapFilename);
            _mapHandler.Init(mapFilename);

            var unitsPath = @"C:\Source\MapEditor\MapEngine\Content\Units\";
            _unitHandler.Init(unitsPath, mapFilename);
        }

        public void Display()
        {
            Update();

            Render();
        }

        private void Update()
        {
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

        private void ProcessInput()
        {

        }
    }
}
