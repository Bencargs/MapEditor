using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Common;
using Common.Entities;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Services.Map;

namespace MapEngine.Services.PathfindingService
{
    public class PathfindingService
    {
        private readonly MapService _map;

        public PathfindingService(MapService mapService)
        {
            _map = mapService;
        }

        public List<Tile> GetPath(Entity entity, Vector2 target)
        {
            var visited = new Dictionary<int, Node<Tile>>();
            var potentials = new NodeQueue<float, Tile>(x => x.Value);

            var destination = _map.GetTile(target);
            var currentTile = _map.GetTile(entity.Location());

            var initialNode = CreateNode(currentTile, destination, null);
            potentials.Push(initialNode);

            bool found;
            Node<Tile> current;
            do
            {
                current = potentials.Pop();

                var mask = GetMask(current.Item, _map.PathfindingTiles);
                foreach (var c in mask)
                {
                    if (!entity.IsNavigable(c) || visited.ContainsKey(c.Id))
                        continue;

                    var node = CreateNode(c, destination, current);
                    visited[c.Id] = node;
                    potentials.Push(node);
                }
                found = current.Item.Equals(destination);
            }
            while (potentials.Any() && !found);

            if (!found)
            {
                // If there is no direct path, get the closest path to destination
                current = visited.Any()
                    ? visited.OrderBy(x => GetDistance(x.Value.Item, destination)).FirstOrDefault().Value
                    : current;
            }

            var path = SimplifyPath(entity, current);

            return path;
        }

        private Node<Tile> CreateNode(Tile tile, Tile destination, Node<Tile> current)
        {
            // todo: include entity speed at tile (type, gradient etc)
            var goalCost = GetDistance(tile, destination);
            var movementCost = current != null
                ? GetDistance(current.Item, tile)
                : 0;

            // todo:
            // issues:
            // - currently a tile's normal/height is just the 0,0 point of the tile
            // anywhere else in the tile may be an obstruction
            // could we include all obstructions in the tile pathing cost?
            // eg a flat tile has no additional cost, lots of change in elevation does?
            // - what about friction? this would enable preferential travel on roads

            return new Node<Tile>(tile)
            {
                Previous = current,
                Value = goalCost + movementCost
            };
        }

        private List<Tile> SimplifyPath(Entity entity, Node<Tile> tail)
        {
            // Removes intermediate nodes and returns an Any-angle path
            var currentIndex = 0;
            var path = tail.ToList();
            var lastIndex = path.Count - 1;

            while (currentIndex < lastIndex - 1)
            {
                var current = path[currentIndex];
                var destination = path[lastIndex];

                if (!IsObstructed(entity, current, destination))
                {
                    // If there is a clear path from current to destination, remove all items between them
                    path.RemoveRange(currentIndex + 1, lastIndex - currentIndex - 1);
                    currentIndex++;
                    lastIndex = path.Count - 1;
                }
                else
                {
                    // If there is an obstruction, increment current and continue
                    lastIndex--;
                }
            }
            return path;
        }

        // todo: move bresenhams ray casting to a common method (duplicate in LoS)
        public bool IsObstructed(Entity entity, Tile source, Tile destination)
        {
            int sourceX = (int)source.Location.X / _map.Scale;
            int sourceY = (int)source.Location.Y / _map.Scale;
            int destX = (int)destination.Location.X / _map.Scale;
            int destY = (int)destination.Location.Y / _map.Scale;

            // Perform raycasting from source to destination via bresenhams

            // Determine the delta values for x and y
            int deltaX = Math.Abs(destX - sourceX);
            int deltaY = Math.Abs(destY - sourceY);

            // Determine the direction of movement in x and y
            int stepX = sourceX < destX ? 1 : -1;
            int stepY = sourceY < destY ? 1 : -1;

            int error = deltaX - deltaY;

            int x = sourceX;
            int y = sourceY;

            while (x != destX || y != destY)
            {
                // Check if the current position encounters a null tile
                if (!entity.IsNavigable(_map.PathfindingTiles[x, y]))
                {
                    return true; // Obstruction found
                }

                int error2 = error * 2;

                if (error2 > -deltaY)
                {
                    error -= deltaY;
                    x += stepX;
                }

                if (error2 < deltaX)
                {
                    error += deltaX;
                    y += stepY;
                }
            }

            return false; // No obstruction found
        }

        private List<Tile> GetMask(Tile current, Tile[,] tiles)
        {
            var gridWidth = tiles.GetLength(0);
            var gridHeight = tiles.GetLength(1);
            var x = (int)current.Location.X / _map.Scale;
            var y = (int)current.Location.Y / _map.Scale;

            var mask = new List<Tile>();

            var startX = Math.Max(x - 1, 0);
            var startY = Math.Max(y - 1, 0);
            var endX = Math.Min(x + 1, gridWidth - 1);
            var endY = Math.Min(y + 1, gridHeight - 1);

            // Iterate over neighboring tiles
            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    if (i == x && j == y) // dont include self in mask
                        continue;

                    var tile = tiles[i, j];
                    mask.Add(tile);
                }
            }

            return mask;
        }

        //// manhattan distance
        //public float GetDistance(Tile source, Tile destination) =>
        //    Math.Abs(source.Location.X - destination.Location.X) + Math.Abs(source.Location.Y - destination.Location.Y);

        // euclidean distance
        public float GetDistance(Tile source, Tile destination) =>
            Vector2.Distance(source.Location, destination.Location);

        //// octile distance
        //	public float GetDistance(Tile source, Tile destination)
        //	{
        //		var dx = Math.Abs(source.Location.X - destination.Location.X);
        //		var dy = Math.Abs(source.Location.Y - destination.Location.Y);
        //
        //		var min = Math.Min(dx, dy);
        //		var max = Math.Max(dx, dy);
        //
        //		var distance = (float)(Math.Sqrt(2) - 1) * min + max;
        //
        //		return distance;
        //	}
    }
}