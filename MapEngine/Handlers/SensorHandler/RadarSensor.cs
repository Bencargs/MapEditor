using Common.Collision;
using System.Linq;
using Common.Entities;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using Common;

namespace MapEngine.Handlers.SensorHandler
{
    public class RadarSensor
    {
        private readonly CollisionHandler _collisionHandler;

        public RadarSensor(CollisionHandler collisionHandler)
        {
            _collisionHandler = collisionHandler;
        }

        public void Update(SensorComponent sensor, Entity entity)
        {
            var hitbox = new BoundingCircle
            {
                Radius = sensor.Radius,
                Location = entity.Location()
            };

            var collisions = _collisionHandler.GetCollisions(hitbox)
                .Where(x => x.entity.Id != entity.Id)
                .Select(x => x.entity);

            sensor.Detections = collisions.ToList();
        }

        public void Render(IGraphics graphics, SensorComponent sensor, Entity entity)
        {
            var location = entity.Location();
            // todo: this should a setting relating sensor type to colour
            var radius = new Rectangle(location, (int)sensor.Radius, (int)sensor.Radius);
            graphics.DrawCircle(new Colour(0, 255, 0, 255), radius);
        }
    }
}
