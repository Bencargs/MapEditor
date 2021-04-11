using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.ResourceLoading;
using SoftEngine;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MapEngine.Handlers
{
    /// <summary>
    /// Responsible for coordinating component handlers
    /// to represent update of entity state
    /// </summary>
    public class EntityHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        // todo.. seperate service..?
        private readonly Device _3dEngine = new Device(new WpfImage(640, 480));

        private readonly MessageHub _messageHub;
        private readonly WeaponHandler _weaponHandler;
        private readonly MovementHandler _movementHandler;
        private readonly SensorHandler _sensorHandler;
        private readonly Dictionary<int, Entity> _entities = new Dictionary<int, Entity>();

        public EntityHandler(
            MessageHub messageHub, 
            MovementHandler movementHandler, 
            SensorHandler sensorHandler,
            WeaponHandler weaponHandler)
        {
            _messageHub = messageHub;
            _sensorHandler = sensorHandler;
            _weaponHandler = weaponHandler;
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
            var playerTeam = 0;
            // todo - really need some extensions around all this component handling
            
            foreach (var unit in _entities.Values)
            {
                var location = unit.GetComponent<LocationComponent>();
                var textureId = unit.GetComponent<ImageComponent>().TextureId;
                if (!TextureFactory.TryGetTexture(textureId, out var texture))
                    continue;

                var team = unit.GetComponent<UnitComponent>().TeamId;
                var sensors = unit.GetComponents<SensorComponent>();
                if (team == playerTeam)
                {
                    // todo - seperate concerns of individual component rendering
                    foreach (var s in sensors)
                    {
                        var radius = new Rectangle(location.Location, (int) s.Radius, (int) s.Radius);
                        graphics.DrawCircle(new Colour(0, 0, 255, 255), radius);
                    }
                }

                // 3d rendering
                if (team == playerTeam || _sensorHandler.IsDetected(playerTeam, unit)) // todo - needs to reconsider this perspective based rendering
                {
                    var modelComponent = unit.GetComponent<ModelComponent>();
                    if (modelComponent != null && ModelFactory.TryGetModel(modelComponent.ModelId, out var model))
                    {
                        model.Location = new Vector3(0, 0, 2);

                        // todo: buggy, weird rotation here
                        //var radians = (Math.PI / 180) * location.FacingAngle;
                        //model.Rotation = new Vector3((float)Math.Cos(radians), (float)Math.Cos(radians), model.Rotation.Z);

                        var render = _3dEngine.Render(model, texture);
                        var tex = new Texture(render);
                        var area = tex.Area(location.Location);
                        area.Translate(viewport.X, viewport.Y);

                        graphics.DrawBytes(render.Buffer, area);
                    }
                    else // 2d rendering
                    {
                        //Translate against camera movement
                        var area = texture.Area(location.Location);
                        area.Translate(viewport.X, viewport.Y);

                        // Rotate image to movement / facing angle
                        // todo: rotate around centre point
                        var rotated = texture.Image.Rotate(location.FacingAngle);

                        graphics.DrawImage(rotated, area);
                    }
                }
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            _entities[command.Entity.Id] = command.Entity;
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity.Id);
        }
    }
}
