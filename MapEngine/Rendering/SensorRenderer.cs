using System.Collections.Generic;
using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;

namespace MapEngine.Rendering
{
    public class SensorRenderer
        : IRenderer
        , IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly List<Entity> _entities = new List<Entity>();

        public void DrawLayer(Rectangle viewport, IGraphics graphics)
        {
            foreach (var entity in _entities)
            {
                // todo: GameSettings.OwnTeam ?
                if (!entity.BelongsTo(Constants.PlayerTeam)) 
                    continue;

                var location = entity.Location();
                var sensors = entity.GetComponents<SensorComponent>();
                foreach (var s in sensors)
                {
                    // todo: this should a setting relating sensor type to colour
                    var radius = new Rectangle(location, (int)s.Radius, (int)s.Radius);
                    graphics.DrawCircle(new Colour(0, 255, 0, 255), radius);
                }
            }
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (entity.GetComponent<SensorComponent>() == null)
                return;

            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
