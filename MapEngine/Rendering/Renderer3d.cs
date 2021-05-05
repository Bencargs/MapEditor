using System.Collections.Generic;
using System.Numerics;
using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.Handlers;
using SoftEngine;

namespace MapEngine.Rendering
{
    public class Renderer3d
        : IRenderer
        , IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        // todo: this is pretty sus - should come from a game settings class
        private readonly Device _3dEngine = new Device(new WpfImage(640, 480));
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly SensorHandler _sensorHandler;

        public Renderer3d(SensorHandler sensorHandler)
        {
            _sensorHandler = sensorHandler;
        }

        public void DrawLayer(Rectangle viewport, IGraphics graphics)
        {
            foreach (var entity in _entities)
            {
                var team = entity.GetComponent<UnitComponent>().TeamId;
                var isDetected = _sensorHandler.IsDetected(Constants.PlayerTeam, entity);
                if (team != Constants.PlayerTeam && !isDetected)
                    continue;

                var modelComponent = entity.GetComponent<ModelComponent>();
                ModelFactory.TryGetModel(modelComponent.ModelId, out var model);

                model.Location = new Vector3(0, 0, 2);
                var location = entity.GetComponent<LocationComponent>();

                // todo: buggy, weird rotation here
                //var radians = (Math.PI / 180) * location.FacingAngle;
                //model.Rotation = new Vector3((float)Math.Cos(radians), (float)Math.Cos(radians), model.Rotation.Z);

                var textureId = entity.GetComponent<ImageComponent>().TextureId;
                TextureFactory.TryGetTexture(textureId, out var texture);
                var render = _3dEngine.Render(model, texture);
                var tex = new Texture(render);
                var area = tex.Area(location.Location);
                area.Translate(viewport.X, viewport.Y);

                graphics.DrawBytes(render.Buffer, area);
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            var textureId = entity.GetComponent<ImageComponent>().TextureId;
            if (!TextureFactory.TryGetTexture(textureId, out _))
                return;

            var modelComponent = entity.GetComponent<ModelComponent>();
            if (modelComponent == null || !ModelFactory.TryGetModel(modelComponent.ModelId, out _))
                return;

            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
