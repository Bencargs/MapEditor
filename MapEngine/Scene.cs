using Common;
using MapEngine.Commands;
using MapEngine.Handlers;
using SoftEngine;

namespace MapEngine
{
    public class Scene
    {
        private readonly IGraphics _graphics;
        private readonly MessageHub _messageHub;
        private readonly MapHandler _mapHandler;
        private readonly EntityHandler _unitHandler;
        private readonly CameraHandler _cameraHandler;
        private readonly Device _3dEngine;

        public Scene(
            IGraphics graphics,
            MessageHub messageHub,
            MapHandler mapHandler, //todo - replace with a handler provider as this grows unweildy?
            EntityHandler unitHandler,
            CameraHandler cameraHandler)
        {
            _graphics = graphics;
            _messageHub = messageHub;
            _mapHandler = mapHandler;
            _unitHandler = unitHandler;
            _cameraHandler = cameraHandler;

            _3dEngine = new Device(new WpfImage(_graphics.Width, _graphics.Height));
        }

        private Mesh[] _mesh;
        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap2.json";
            _cameraHandler.Initialise(mapFilename);
            _mapHandler.Initialise(mapFilename);

            var weaponsPath = @"C:\Source\MapEditor\MapEngine\Content\Weapons\";
            var unitsPath = @"C:\Source\MapEditor\MapEngine\Content\Units\";
            _unitHandler.Initialise(unitsPath, mapFilename, weaponsPath);

            // todo: figure out why its not rotated and possitioned correctly
            _mesh = ObjectLoader.LoadJSONFileAsync(
                @"C:\Source\MapEditor\MapEngine\Content\Models\cube.babylon",
                @"C:\Source\MapEditor\MapEngine\Content\Textures\Cube.png");
            _mesh[0].Rotate(0.5f, 0.2f, 0.1f); //293.74f, 318.60f, 303.45f
            _mesh[0].Translate(0, 0, 2);
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

            var render = _3dEngine.Render(_mesh[0]);
            _graphics.DrawBytes(render);

            _graphics.Render();
        }
    }
}
