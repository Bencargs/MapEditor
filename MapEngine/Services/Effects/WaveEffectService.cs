using Common;
using MapEngine.Services.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapEngine.Services.Effects
{
    public class WaveEffectService
    {
        private class WaterParticle
        {
            public float Height { get; set; }
            public float Velocity { get; set; }
            public float Acceleration { get; set; }
			public float Sustainability { get; set; }
		}

        private readonly float mass = 0.1f;
        private readonly float maxHeight = 500f;
        private readonly float resolution = 5f;
		private readonly float sustain = 1000f;

		// Simulation dimentions
		private int width;
		private int height;
		// Render dimetions
		private int mapWidth;
		private int mapHeight;

		private WaterParticle[,] _grid;
		private Queue<(int x, int y)> _queue;

		private readonly MapService _mapService;

		public WaveEffectService(MapService mapService)
		{
			_mapService = mapService;
		}

		public void Initialise()
		{
			width = _mapService.Tiles.GetLength(0);
			height = _mapService.Tiles.GetLength(1);
			mapWidth = _mapService.Width;
			mapHeight = 640;// _mapService.Height; todo: wha??

			_queue = new Queue<(int x, int y)>(width * height);
			_grid = new WaterParticle[width, height];
			foreach (var tile in _mapService.Tiles)
			{
				if (tile.Type != TerrainType.Water)
					continue;

				var x = (int)tile.Location.X / 4;
				var y = (int)tile.Location.Y / 4;
				_grid[x, y] = new WaterParticle
				{
					Height = 0,
					Velocity = 0,
					Acceleration = 0,
					Sustainability = sustain
				};
			}
		}

		public void SetHeight(int x, int y, float value)
		{
			if (x < 0)
				x = 0;
			else if (x > width - 1)
				x = width - 1;

			if (y < 0)
				y = 0;
			else if (y > height - 1)
				y = height - 1;

			var cell = _grid[x, y];
			if (cell == null)
				return;

			cell.Height = value;
			_queue.Enqueue((x, y));
		}

		public void CalculateForces()
		{
			float total_height = 0;

			var updates = new HashSet<(int x, int y)>();
			while (_queue.Any())
			{
				var (x, y) = _queue.Dequeue();
				var Value = _grid[x, y];
				
				Value.Acceleration = 0;
				total_height += Value.Height;

				float heights = 0;
				int num_of_part = 0;
				foreach (var particle in GetMask(x, y).Where(p => p.Value != null))
				{
					heights += particle.Value.Height;
					num_of_part++;

					updates.Add((particle.x, particle.y));
				}

				heights /= num_of_part;
				Value.Acceleration += -(Value.Height - heights) / mass;
				Value.Acceleration -= Value.Velocity / Value.Sustainability;

				if (Value.Acceleration > maxHeight)
					Value.Acceleration = maxHeight;
				else if (Value.Acceleration < -maxHeight)
					Value.Acceleration = -maxHeight;
			}
			_queue.Enqueue(updates);

			float shifting = -total_height / _grid.Length;
			foreach (var (x, y) in _queue)
			{
				var Value = _grid[x, y];
				Value.Velocity += Value.Acceleration;

				if (Value.Height + Value.Velocity / resolution > maxHeight)
					Value.Height = maxHeight;
				else if (Value.Height + Value.Velocity / resolution <= maxHeight && Value.Height + Value.Velocity / resolution >= -maxHeight)
					Value.Height += Value.Velocity / resolution;
				else
					Value.Height = -maxHeight;

				Value.Height += shifting;
			}
		}

		private IEnumerable<(int x, int y, WaterParticle Value)> GetMask(int x, int y)
		{
			if (x > 0)
				yield return (x-1, y, _grid[x - 1, y]);

			if (x > 0 && y > 0)
				yield return (x-1, y-1, _grid[x - 1, y - 1]);

			if (y > 0)
				yield return (x, y-1, _grid[x, y - 1]);

			if (y > 0 && x < width - 1)
				yield return (x+1, y-1, _grid[x + 1, y - 1]);

			if (x < width - 1)
				yield return (x+1, y, _grid[x + 1, y]);

			if (x < width - 1 && y < height - 1)
				yield return (x+1, y+1,_grid[x + 1, y + 1]);

			if (y < height - 1)
				yield return (x, y+1, _grid[x, y + 1]);

			if (y < height - 1 && x > 0)
				yield return (x-1, y+1, _grid[x - 1, y + 1]);
		}

		public byte[] GenerateBitmap()
		{
			var xScale = (float)mapWidth / width;
			var yScale = (float)mapHeight / height;

			var i = 0;
			var bytes = new byte[mapWidth * mapHeight];
			for (int x = 0; x < mapWidth; x++)
			{
				for (int y = 0; y < mapHeight; y++)
				{
					// todo: whats going on here..?
					var x1 = (int)(x / xScale);
					var y1 = Math.Min(126, (int)(y / yScale));
					var value = _grid[y1, x1];

					byte alpha = value != null
						? (byte)((value.Height + maxHeight) / (maxHeight * 2f / 255f))
						: (byte)0;

					bytes[i] = alpha;
					i++;
				}
			}

			return bytes;
		}

		//public byte[] GenerateBitmap()
		//{
		//	var xScale = (float) mapWidth / width;
		//	var yScale = (float) mapHeight / height;

		//	var i = 0;
		//	var bytes = new byte[mapWidth * 4 * mapHeight];
		//	for (int x = 0; x < mapWidth; x++)
		//	{
		//		for (int y = 0; y < mapHeight; y++)
		//		{
		//			// todo: whats going on here..?
		//			var x1 = (int)(x / xScale);
		//			var y1 = Math.Min(126, (int)(y / yScale));
		//			var value = _grid[y1, x1];

		//			byte alpha = value != null
		//				? (byte)((value.Height + maxHeight) / (maxHeight * 2f / 255f))
		//				: (byte)0;

		//			bytes[i] = alpha;
		//			bytes[i + 1] = alpha;
		//			bytes[i + 2] = alpha;
		//			bytes[i + 3] = alpha;
		//			i += 4;
		//		}
		//	}

		//	return bytes;
		//}
	}
}
