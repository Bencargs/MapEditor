using Common;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Services.Map;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MapEngine.Services.Navigation
{
    public class NavigationService
    {
        private readonly MapService _map;

        public NavigationService(MapService mapService)
        {
            _map = mapService;
        }

        public Tile[] GetPath(Entity entity, Vector2 target)
        {
            var location = entity.GetComponent<LocationComponent>().Location;
            var movement = entity.GetComponent<MovementComponent>();

            var visited = new Dictionary<int, Node<Tile>>();
            var potentials = new NodeQueue<int, float, Tile>(x => x.Value);

            var destination = _map.GetTile(target);
            var currentTile = _map.GetTile(location);
            var initialNode = CreateNode(currentTile, destination, null);
            potentials.Push(initialNode);

            var found = false;
            Node<Tile> current = null;
            do
            {
                current = potentials.Pop();

                var mask = GetMask(movement, current.Item, _map.Tiles);
                foreach (var c in mask)
                {
                    if (c == null || visited.ContainsKey(c.Id))
                        continue;

                    var node = CreateNode(c, destination, current);
                    visited.Add(c.Id, node);
                    potentials.Push(node);
                }
                found = current.Item == destination;
            }
            while (potentials.Any() && !found);

            if (!found)
            {
                // If there is no direct path, get the closest path to destination
                current = visited.OrderBy(x => x.Value.Value).FirstOrDefault().Value;
            }

            return Prune(current).ToArray();
        }

        private Node<Tile> CreateNode(Tile tile, Tile destination, Node<Tile> current)
        {
            var goalCost = GetDistance(tile, destination);
            var movementCost = current != null
                ? GetDistance(current.Item, tile)
                : 0;

            return new Node<Tile>(tile)
            {
                Previous = current,
                Value = goalCost + movementCost
            };
        }

        private Node<Tile> Prune(Node<Tile> tail)
        {
            var current = tail;

            while (current.Previous != null && current.Previous.Previous != null)
            {
                var temp = current.Previous.Previous;

                // if two points have the same slope, they are co-linear
                var currentSlope = (current.Previous.Item.Location.Y - current.Item.Location.Y) / (current.Previous.Item.Location.X - current.Item.Location.X);
                var previousSlope = (temp.Item.Location.Y - current.Previous.Item.Location.Y) / (temp.Item.Location.X - current.Previous.Item.Location.X);

                // if all three points exist on the same line, remove the middle point
                if (currentSlope == previousSlope)
                    current.Previous = temp;
                else
                    current = current.Previous;
            }

            return tail;
        }

        private List<Tile> GetMask(MovementComponent movementComponent, Tile current, Tile[,] tiles)
        {
            var (currentX, currentY) = _map.GetCoordinates(current);

            var results = new List<Tile>();
            foreach (var (X, Y) in movementComponent.MovementMask)
            {
                if (currentX < 0 || currentY < 0 ||
                    currentX > tiles.GetLength(0) - 1 ||
                    currentY > tiles.GetLength(1) - 1)
                    continue;

                var tile = tiles[currentX + X, currentY + Y];
                if (movementComponent.Terrains.Contains(tile.Type))
                    results.Add(tile);
            }
            return results;
        }

        //// manhattan distance
        //public float GetDistance(Tile source, Tile destination) =>
        //    Math.Abs(source.Location.X - destination.Location.X) + Math.Abs(source.Location.Y - destination.Location.Y);

        // euclidean distance
        public float GetDistance(Tile source, Tile destination) =>
            Vector2.Distance(source.Location, destination.Location);
    }
}