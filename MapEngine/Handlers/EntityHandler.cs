using System.Numerics;
using Common;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.ResourceLoading;
using MapEngine.Rendering;
using MapEngine.Services.Map;
using MapEngine.Handlers.SensorHandler;

namespace MapEngine.Handlers
{
    /// <summary>
    /// Responsible for coordinating component handlers
    /// to represent update of entity state
    /// </summary>
    public class EntityHandler // todo: rename entityManager?
        : IHandleCommand<CreateEntityCommand>
    {
        private readonly MessageHub _messageHub;
        private readonly WeaponHandler _weaponHandler;
        private readonly MovementHandler _movementHandler;
        private readonly CollisionHandler _collisionHandler;
        private readonly SensorHandler.SensorHandler _sensorHandler;
        private readonly MapService _mapService;
        private readonly IRenderer _2dRenderer;
        private readonly IRenderer _3dRenderer;

        public EntityHandler(
            MessageHub messageHub, 
            MovementHandler movementHandler, 
            CollisionHandler collisionHandler,
            SensorHandler.SensorHandler sensorHandler,
            WeaponHandler weaponHandler,
            MapService mapService,
            Renderer2d renderer2d, // todo: RenderFactory.GetRenderers?
            Renderer3d renderer3d)
        {
            _messageHub = messageHub;
            _sensorHandler = sensorHandler;
            _weaponHandler = weaponHandler;
            _2dRenderer = renderer2d;
            _3dRenderer = renderer3d;
            _movementHandler = movementHandler;
            _collisionHandler = collisionHandler;
            _mapService = mapService;
        }

        public void Initialise(string unitsFilepath, string mapFilename, string weaponFilepath, string modelFilepath, string particleFilepath)
        {
            // todo: refactor this to: 
            // unit = LoadUnitModel(); 
            // LoadTexture(unit.Texture);
            // factories.Initialise..?
            ParticleFactory.LoadParticles(particleFilepath);
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
            _collisionHandler.Update();
            _sensorHandler.Update();
            _weaponHandler.Update();
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            _sensorHandler.DrawLayer(viewport, graphics);
            _3dRenderer.DrawLayer(viewport, graphics);
            _2dRenderer.DrawLayer(viewport, graphics);
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            var mapHeight = _mapService.GetHeight(entity.Location());
            entity.GetComponent<LocationComponent>().Height = mapHeight;
        }
    }
}
