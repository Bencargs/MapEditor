using System.Collections.Generic;
using System.Linq;
using Common.Collision;
using Common.Entities;
using MapEngine.Commands;
using MapEngine.Entities.Components;

namespace MapEngine.Handlers
{
    public class SensorHandler
        : IHandleCommand<CreateEntityCommand>
        , IHandleCommand<DestroyEntityCommand>
    {
        private readonly CollisionHandler _collisionHandler;
        private readonly List<Entity> _entities = new List<Entity>();
        
        public SensorHandler(CollisionHandler collisionHandler)
        {
            _collisionHandler = collisionHandler;
        }

        public bool IsDetected(int team, Entity entity)
        {
            var detections = _entities
                .Where(x => x.GetComponent<UnitComponent>().TeamId == team)
                .Select(x => x.GetComponent<SensorComponent>())
                .Select(x => x.Detections.Contains(entity));

            return detections.Any(x => x);
        }

        public void Update()
        {
            foreach (var e in _entities)
            {
                var location = e.GetComponent<LocationComponent>().Location;
                var sensorComponents = e.GetComponents<SensorComponent>();
                foreach (var s in sensorComponents)
                {
                    // todo - collisioncomponent.getcollider?
                    var hitbox = new BoundingCircle
                    {
                        Radius = s.Radius,
                        Location = location
                    };

                    var collisions = _collisionHandler.GetCollisions(hitbox)
                        .Where(x => x.entity.Id != e.Id)
                        .Select(x => x.entity);

                    s.Detections = collisions.ToList();
                }
            }
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
