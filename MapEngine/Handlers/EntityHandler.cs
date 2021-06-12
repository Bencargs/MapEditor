using Common;
using MapEngine.Commands;
using MapEngine.Factories;
using MapEngine.ResourceLoading;
using MapEngine.Rendering;

namespace MapEngine.Handlers
{
    /// <summary>
    /// Responsible for coordinating component handlers
    /// to represent update of entity state
    /// </summary>
    public class EntityHandler // todo: rename entityManager?
    {
        private readonly MessageHub _messageHub;
        private readonly WeaponHandler _weaponHandler;
        private readonly MovementHandler _movementHandler;
        private readonly SensorHandler _sensorHandler;
        private readonly IRenderer _2dRenderer;
        private readonly IRenderer _3dRenderer;
        private readonly IRenderer _sensorRenderer;

        public EntityHandler(
            MessageHub messageHub, 
            MovementHandler movementHandler, 
            SensorHandler sensorHandler,
            WeaponHandler weaponHandler,
            Renderer2d renderer2d, // todo: RenderFactory.GetRenderers?
            Renderer3d renderer3d,
            SensorRenderer sensorRenderer)
        {
            _messageHub = messageHub;
            _sensorHandler = sensorHandler;
            _weaponHandler = weaponHandler;
            _2dRenderer = renderer2d;
            _3dRenderer = renderer3d;
            _sensorRenderer = sensorRenderer;
            _movementHandler = movementHandler;
        }

        public void Initialise(string unitsFilepath, string mapFilename, string weaponFilepath, string modelFilepath)
        {
            // todo: refactor this to: 
            // unit = LoadUnitModel(); 
            // LoadTexture(unit.Texture);
            // factories.Initialise..?
            TextureFactory.LoadTextures(@"C:\Source\MapEditor\MapEngine\Content\Textures");
            WeaponFactory.LoadWeapons(weaponFilepath); // todo: code stink - requires factories to be initialised in an order
            UnitFactory.LoadUnits(unitsFilepath);
            ModelFactory.LoadModel(modelFilepath);

            var units = UnitLoader.LoadUnits(mapFilename);
            foreach (var unit in units)
            {
                _messageHub.Post(new CreateEntityCommand { Entity = unit });
            }
        }

        public void Update()
        {
            _movementHandler.Update();
            _sensorHandler.Update();
            _weaponHandler.Update();
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            _3dRenderer.DrawLayer(viewport, graphics);
            _2dRenderer.DrawLayer(viewport, graphics);
            _sensorRenderer.DrawLayer(viewport, graphics);
        }
    }
}
