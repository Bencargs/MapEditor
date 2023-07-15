using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common.Entities;
using MapEngine.Entities;

namespace MapEngine.Handlers
{
    public class FormationService
    {
        // todo: add an enum here for type of position, circular, triangle, grid etc
        public static Dictionary<Entity, Vector2> GetFormationPositions(List<Entity> entities, Vector2 destination)
        {
            var unitAreas = entities.Select(x => x.Texture()).ToList();
            var positions = GetCircleFormation(unitAreas)
                .Select(p => p + destination)
                .ToList();

            var paths = GetPaths(entities, positions);

            return paths.ToDictionary(k => k.Source, v => v.Destination);
        }

        private static List<(Entity Source, Vector2 Destination)> GetPaths(List<Entity> entities, List<Vector2> positions)
        {
            var entityPositionPairs = new List<(Entity Source, Vector2 Destination)>();

            // find the centroid of the positions list
            var center = new Vector2(
                x: positions.Average(p => p.X), 
                y: positions.Average(p => p.Y));

            // Sort the entities based on their distance to the center of positions in descending order
            var sortedSquares = entities.OrderByDescending(s => Vector2.Distance(s.Location(), center)).ToList();

            // Pair each square with the closest available position
            foreach (var square in sortedSquares)
            {
                var closestDistance = positions.OrderBy(p => Vector2.Distance(square.Location(), p)).First();

                // Add the square-position pair to the map
                entityPositionPairs.Add((square, closestDistance));

                // Remove the assigned position from the available positions
                positions.Remove(closestDistance);
            }

            return entityPositionPairs;
        }

        // todo: formation service?
        private static List<Vector2> GetCircleFormation(List<Texture> units)
        {
            // Calculate the radius that can accommodate at least numPoints
            var numPoints = units.Count;
            int radius = (int)Math.Ceiling(Math.Sqrt(numPoints / Math.PI));

            // Gets all the points in a circle of that radius
            List<Vector2> allPoints = CalculateCirclePoints(radius);

            var centerPoint = new Vector2(0, 0);
            // Sort the points based on their Euclidean distance to the center
            var orderedPoints = allPoints.OrderBy(p => Vector2.Distance(p, centerPoint))
                .Take(numPoints)
                .ToList();

            // Multiply each point location by the square width and take the center of each square
            float squareWidth = units[0].Width; // todo: assumes all entities have the same width
            List<Vector2> squareCenters = orderedPoints
                .Select((p, i) => new Vector2(p.X * squareWidth + squareWidth / 2, p.Y * squareWidth + squareWidth / 2))
                .ToList();

            return squareCenters;
        }

        // todo: move to an extension class
        private static List<Vector2> CalculateCirclePoints(int radius)
        {
            List<Vector2> points = new List<Vector2>();

            int x = 0;
            int y = radius;
            int d = 1 - radius;

            while (x <= y)
            {
                // Add points in all eight octants of the circle
                for (int i = -x; i <= x; i++)
                {
                    points.Add(new Vector2(i, y));
                    points.Add(new Vector2(i, -y));
                }

                for (int i = -y; i <= y; i++)
                {
                    points.Add(new Vector2(i, x));
                    points.Add(new Vector2(i, -x));
                }

                if (d < 0)
                {
                    d += 2 * x + 3;
                }
                else
                {
                    d += 2 * (x - y) + 5;
                    y--;
                }

                x++;
            }

            return points.Distinct().ToList();
        }
    }
}
