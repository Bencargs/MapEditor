using Common;
using MapEngine.Handlers;
using SoftEngine;

namespace MapEngine
{
    public class Scene
    {
        private readonly IGraphics _graphics;
        private readonly UnitHandler _unitHandler;
        private readonly MapHandler _mapHandler;

        public Scene(IGraphics graphics)
        {
            _graphics = graphics;

            // replace with injected singleton
            var textures = new TextureHandler();

            _mapHandler = new MapHandler(textures);
            _unitHandler = new UnitHandler(textures);
        }

        private IImage _render;
        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap1.json";
            _mapHandler.Init(mapFilename);

            var unitFilename = @"C:\Source\MapEditor\MapEngine\Content\Units\Dummy.json";
            _unitHandler.Init(unitFilename);

            var modelFilename = @"C:\Source\MapEditor\MapEngine\Content\Models\monkey.babylon";
            var textureFilename = @"C:\Source\MapEditor\MapEngine\Content\Textures\Suzanne.png";
            _render = new WpfImage(_graphics.Width, _graphics.Height);
            var modelRenderer = new Device(_render);
            var model = ObjectLoader.LoadJSONFileAsync(modelFilename, textureFilename);
            modelRenderer.Render(model);
        }

        public void Display()
        {
            Render();
        }

        private void Update()
        {
            
        }

        private void Render()
        {
            _graphics.Clear();

            _mapHandler.Render(_graphics);
            //_unitHandler.Render(_graphics);
            _graphics.DrawImage(_render.Scale(0.99), new Rectangle(0, 0, _graphics.Width, _graphics.Height));


            _graphics.Render();
        }

        private void ProcessInput()
        {

        }
    }
}
