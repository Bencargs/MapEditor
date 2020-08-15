using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.ResourceLoading;
using SoftEngine;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapEngine.Handlers
{
    /// <summary>
    /// Responsible for corrdinating component handlers
    /// to represent update of entity state
    /// </summary>
    public class EntityHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        // todo.. seperate handler..?
        private readonly Device _3dEngine = new Device(new WpfImage(640, 480));

        private readonly MessageHub _messageHub;
        private readonly WeaponHandler _weaponHandler;
        private readonly MovementHandler _movementHandler;
        private readonly List<Entity> _entities = new List<Entity>();

        public EntityHandler(MessageHub messageHub, MovementHandler movementHandler, WeaponHandler weaponHandler)
        {
            _messageHub = messageHub;
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
            _weaponHandler.Update();
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            foreach (var unit in _entities)
            {
                var location = unit.GetComponent<LocationComponent>();
                var textureId = unit.GetComponent<ImageComponent>().TextureId;
                if (!TextureFactory.TryGetTexture(textureId, out var texture))
                    continue;

                // 3d rendering
                var modelComponent = unit.GetComponent<ModelComponent>();
                if (modelComponent != null && ModelFactory.TryGetModel(modelComponent.ModelId, out var model))
                {
                    model.Rotation = new Vector3(model.Rotation.X + 0.02f, model.Rotation.Y + 0.09f, model.Rotation.Z);
                    model.Location = new Vector3(0, 0, 2);

                    //var area = texture.Area(location.Location);
                    //area.Translate(viewport.X, viewport.Y);

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

        public void Handle(CreateEntityCommand command)
        {
            _entities.Add(command.Entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
