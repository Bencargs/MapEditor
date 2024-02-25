using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities;
using MapEngine.Entities.Components;

namespace MapEngine.Handlers.SensorHandler
{
    public class SensorHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly RadarSensor _radarSensor;
        private readonly SightSensor _sightSensor;


        public SensorHandler(
            RadarSensor radarSensor, 
            SightSensor sightSensor)
        {
            _radarSensor = radarSensor;
            _sightSensor = sightSensor;
        }

        public bool IsDetected(int team, Entity entity)
        {
            var detections = _entities
                .Where(x => x.BelongsTo(team))
                .Select(x => x.GetComponent<SensorComponent>())
                .Select(x => x.Detections.Contains(entity));

            return detections.Any(x => x);
        }

        public void Update()
        {
            foreach (var e in _entities)
            {
                var sensorComponents = e.GetComponents<SensorComponent>();
                foreach (var s in sensorComponents)
                {
                    // todo: strategy pattern?
                    switch (s.Name)
                    {
                        case "Radar":
                            _radarSensor.Update(s, e);
                            break;
                        case "Sight":
                            _sightSensor.Update(s, e);
                            break;

                    }
                }
            }
        }

        public float[] GenerateBitmap(Rectangle viewport, IGraphics graphics)
        {
            var fieldOfView = new float[viewport.Width * viewport.Height];
            for (int i = 0; i < fieldOfView.Length; i++) fieldOfView[i] = 0.25f;

            foreach (var entity in _entities)
            {
                // todo: GameSettings.OwnTeam ?
                if (!entity.BelongsTo(Constants.PlayerTeam))
                    continue;

                var sensors = entity.GetComponents<SensorComponent>();
                foreach (var s in sensors)
                {
                    switch (s.Name)
                    {
                        case "Radar":
                            _radarSensor.Render(graphics, s, entity);
                            break;
                        case "Sight":
                            _sightSensor.Render(fieldOfView, viewport.Width, s);
                            break;
                    }
                }
            }

            return fieldOfView;
        }

        public void Handle(CreateEntityCommand command)
        {
            var entity = command.Entity;
            if (!entity.GetComponents<SensorComponent>().Any())
                return;
            
            _entities.Add(entity);
        }

        public void Handle(DestroyEntityCommand command)
        {
            _entities.Remove(command.Entity);
        }
    }
}
