using System.Collections.Generic;
using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Handlers.SensorHandler;

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
                var location = entity.GetComponent<LocationComponent>();
                if (!viewport.Contains(location.Location))
                    continue;

                // If enemy unit hasn't been detected, don't draw
                if (!entity.BelongsTo(Constants.PlayerTeam) && 
                    !_sensorHandler.IsDetected(Constants.PlayerTeam, entity))
                    continue;

                // Rotate image to movement / facing angle
                var texture = entity.Texture();
                var rotated = texture.Image.Rotate(location.FacingAngle);
                
                // Translate against camera movement
                var area = rotated.Area(location.Location);
                area.Translate(viewport.X, viewport.Y);

                graphics.DrawImage(rotated, area);
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (entity.Texture() == null)
                return;
            
            if (entity.Model() != null)
                return; // don't render 3d models here
            
            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
