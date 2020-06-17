using Common;
using MapEngine.Handlers;

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

        public void Initialise()
        {
            var mapFilename = @"C:\Source\MapEditor\MapEngine\Content\Maps\TestMap1.json";
            _mapHandler.Init(mapFilename);

            var unitFilename = @"C:\Source\MapEditor\MapEngine\Content\Units\Dummy.json";
            _unitHandler.Init(unitFilename);
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
            _unitHandler.Render(_graphics);

            _graphics.Render();
        }

        private void ProcessInput()
        {

        }
    }
}
