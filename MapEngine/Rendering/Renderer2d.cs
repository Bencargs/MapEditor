using System.Collections.Generic;
using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.Handlers;

namespace MapEngine.Rendering
{
    public class Renderer2d
        : IRenderer
        , IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly SensorHandler _sensorHandler;
        private readonly List<Entity> _entities = new List<Entity>();

        public Renderer2d(SensorHandler sensorHandler)
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

                var location = entity.GetComponent<LocationComponent>();
                var textureId = entity.GetComponent<ImageComponent>().TextureId;
                TextureFactory.TryGetTexture(textureId, out var texture);

                //Translate against camera movement
                var area = texture.Area(location.Location);
                area.Translate(viewport.X, viewport.Y);

                // Rotate image to movement / facing angle
                // todo: rotate around centre point
                var rotated = texture.Image.Rotate(location.FacingAngle);

                graphics.DrawImage(rotated, area);
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            var textureId = entity.GetComponent<ImageComponent>().TextureId;
            if (!TextureFactory.TryGetTexture(textureId, out _))
                return;

            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
