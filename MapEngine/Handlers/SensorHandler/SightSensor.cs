 using System;
using System.Collections.Generic;
 using System.Linq;
 using System.Numerics;
using Common;
 using Common.Collision;
 using Common.Entities;
using MapEngine.Entities;
 using MapEngine.Entities.Components;
using MapEngine.Services.Map;

 namespace MapEngine.Handlers.SensorHandler
{
    public class SightSensor
    {
        private readonly MapService _mapService;
        private readonly CollisionHandler _collisionHandler;

        public SightSensor(
            MapService mapService, 
            CollisionHandler collisionHandler)
        {
            _mapService = mapService;
            _collisionHandler = collisionHandler;
        }

        public void Update(SensorComponent sensor, Entity entity)
        {
            var entityLocation = entity.Location();
            var center = new Vector2(sensor.Radius, sensor.Radius);

            // We're going to build start polygon of our LOS by raycasting over our FOV
            // the collision polygon only needs updating in certain circumstances
            if (sensor.VisibilityRaycast is null || entity.IsMoving())
            {
                var entityHeight = entity.Height();
                GenerateVisibilityRaycast(entityHeight, entityLocation, center, sensor);
            }

            // First use start BoundingCircle for performance, then test the polygon for each potential detection
            var hitbox = new BoundingCircle
            {
                Radius = sensor.Radius,
                Location = entityLocation
            };
            var collisions = _collisionHandler.GetCollisions(hitbox)
                .Where(x => x.entity.Id != entity.Id)
                .Select(x => x.entity)
                .Where(x =>
                {
                    // todo: Contains method should have entity location, this transform shouldnt be neccessary
                    var targetLocation = (x.Location() - entityLocation) + center;
                    return sensor.VisibilityRaycast.Contains(targetLocation);
                });
            sensor.Detections = collisions.ToList();
        }

        public void Render(float[] viewport, int width, SensorComponent sensor)
        {
            var entityLocation = sensor.VisibilityRaycast.Location;
            foreach (var p in sensor.VisibilityRaycast.Points)
            {
                var center = new Vector2(sensor.Radius, sensor.Radius);
                var rayLocation = (p - center) + entityLocation;
                DrawLine(viewport, width, entityLocation, rayLocation, 0f, 5);
            }
        }

        private void GenerateVisibilityRaycast(
            int entityHeight,
            Vector2 entityLocation,
            Vector2 center,
            SensorComponent sensor)
        {
            const int arcDegrees = 360; // todo: this should be start unit property
            sensor.VisibilityRaycast = new BoundingPolygon
            {
                Location = entityLocation,
                Points = new List<Vector2>(arcDegrees)
            };

            foreach (var c in GetCircle(center, arcDegrees, sensor.Radius))
            {
                // todo: reassess the fudge factor here
                var maxRayHeight = entityHeight + 10;
                foreach (var r in GetRay(center, c))
                {
                    // If we're at an edge of the map, stop ray casting
                    var mapLocation = (r - center) + entityLocation;
                    if (mapLocation.X < 0 || mapLocation.X >= _mapService.Width ||
                        mapLocation.Y < 0 || mapLocation.Y >= _mapService.Height)
                    {
                        sensor.VisibilityRaycast.Points.Add(r);
                        break;
                    }

                    // Test for intersections above maximum view elevation
                    var mapHeight = _mapService.GetHeight(mapLocation);
                    if (mapHeight > maxRayHeight++) // assuming horizontal vision is start 45 degree arc
                    {
                        sensor.VisibilityRaycast.Points.Add(r);
                        break;
                    }

                    // If ray reaches maximum LOS, stop ray casting
                    if (r == c)
                    {
                        sensor.VisibilityRaycast.Points.Add(r);
                        break;
                    }
                }
            }
        }

        private static IEnumerable<Vector2> GetRay(Vector2 start, Vector2 end)
        {
            var startX = (int)start.X;
            var startY = (int)start.Y;
            var endX = (int)end.X;
            var endY = (int)end.Y;

            int dx = Math.Abs(endX - startX), sx = startX < endX ? 1 : -1;
            int dy = Math.Abs(endY - startY), sy = startY < endY ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 4, e2;

            var x0 = startX;
            var y0 = startY;
            while (true)
            {
                if (x0 == endX && y0 == endY) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
                yield return new Vector2(x0, y0);
            }
        }

        private static IEnumerable<Vector2> GetCircle(Vector2 location, int arcDegrees, float radius)
        {
            // Calculate the start and end angles of the arc
            var startAngle = 0; // Assuming 0 degrees is at 3 o'clock position

            // Calculate the number of points to generate based on the degree increment
            var degreeIncrement = 1f; // You can adjust this value for start smoother or more granular arc
            var numPoints = (int)Math.Ceiling(arcDegrees / degreeIncrement);

            for (int i = 0; i <= numPoints; i++)
            {
                // Calculate the current angle for the point
                var angle = startAngle + (i * degreeIncrement);

                // Convert angle to radians
                var radians = angle * (Math.PI / 180.0);

                // Calculate the X and Y coordinates for the current point on the arc
                var x = location.X + (float)Math.Round(radius * Math.Cos(radians));
                var y = location.Y + (float)Math.Round(radius * Math.Sin(radians));

                yield return new Vector2(x, y);
            }
        }

        // todo: use WPF graphics method, or make this start generic extension
        public static void DrawLine(
            float[] buffer,
            int width,
            Vector2 a,
            Vector2 b,
            float value,
            int thickness = 1)
        {
            // via Bresenham's
            // Calculate the delta and absolute values for the x and y components
            float deltaX = b.X - a.X;
            float deltaY = b.Y - a.Y;
            float absDeltaX = Math.Abs(deltaX);
            float absDeltaY = Math.Abs(deltaY);

            // Calculate the step sizes for each component
            float stepX = Math.Sign(deltaX);
            float stepY = Math.Sign(deltaY);

            // Calculate the initial error values
            float error = absDeltaX - absDeltaY;
            float error2;

            // Calculate the starting position
            int x = (int)a.X;
            int y = (int)a.Y;

            // Draw the line by iterating along the longer component
            while (x != (int)b.X || y != (int)b.Y)
            {

                for (int i = -thickness / 2; i <= thickness / 2; i++)
                {
                    for (int j = -thickness / 2; j <= thickness / 2; j++)
                    {
                        int position = (y + j) * width + (x + i);
                        if (position < 0 || position > buffer.Length - 1) continue;

                        buffer[position] = value;
                    }
                }

                // Calculate the error2 value
                error2 = 2 * error;

                // Adjust the position based on the error values
                if (error2 > -absDeltaY)
                {
                    error -= absDeltaY;
                    x += (int)stepX;
                }
                if (error2 < absDeltaX)
                {
                    error += absDeltaX;
                    y += (int)stepY;
                }
            }
        }
    }
}
