<Query Kind="Program">
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

void Main()
{
	// make a big search space
	//var array = new Tile[192, 128];
	//for (int x = 0; x < 192; x++)
	//	for (var y = 0; y < 128; y++)
	//	{
	//		array[x, y] = new Tile
	//		{
	//			Id = y * 192 + x,
	//			Location = new Vector2(x * 4, y * 4),
	//		};
	//	}
	//// throw in some obsticles
	//for (int i = 20; i < 168; i++)
	//{
	//	array[i, 64] = null;
	//}
	//for (int i = 20; i < 128; i++)
	//{
	//	array[96, i] = null;
	//}

	var array = new MazeGenerator(192, 128).GenerateMaze();

	var map = new MapService(array);

	//var origin = new Vector2(10, 11);
	//var destination = new Vector2(750, 505);
	var origin = new Vector2(0, 0);
	var destination = new Vector2(190 * 4, 126 * 4);
	var pfs = new PathfindingService(map);
	var path = pfs.GetPath(origin, destination);
}

public class MazeGenerator
{
	private Tile[,] maze;
	private int width;
	private int height;
	private Random random;

	public MazeGenerator(int width, int height)
	{
		this.width = width;
		this.height = height;
		maze = new Tile[width, height];
		random = new Random(42);
	}

	public Tile[,] GenerateMaze()
	{
		// Initialize the maze with walls
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				maze[x, y] = null;
			}
		}

		// Create a random starting point
		GenerateMazeRecursive(0, 0);

		return maze;
	}

	private void GenerateMazeRecursive(int x, int y)
	{
		maze[x, y] = new Tile
		{
			Id = y * 192 + x,
			Location = new Vector2(x * 4, y * 4),
		}; // Set the current position as a floor

	    // Generate a random direction order (up, down, left, right)
	    int[] directions = { 0, 1, 2, 3 };
	    Shuffle(directions);

	    foreach (int direction in directions)
	    {
	        int nextX = x;
	        int nextY = y;

	        // Move in the selected direction
	        switch (direction)
	        {
	            case 0: // Up
	                nextY -= 2;
	                break;
	            case 1: // Down
	                nextY += 2;
	                break;
	            case 2: // Left
	                nextX -= 2;
	                break;
	            case 3: // Right
	                nextX += 2;
	                break;
	        }

	        // Check if the next position is valid
	        if (IsValidPosition(nextX, nextY))
	        {
				// Carve a path by removing the wall between the current and next positions
				maze[nextX, nextY] = new Tile
				{
					Id = nextY * 192 + nextX,
					Location = new Vector2(nextX * 4, nextY * 4),
				};
				maze[(nextX + x) / 2, (nextY + y) / 2] = new Tile
				{
					Id = nextY * 192 + nextX,
					Location = new Vector2(nextX * 4, nextY * 4),
				};

					// Recursively generate the maze from the next position
					GenerateMazeRecursive(nextX, nextY);
				}
			}
		}

	private bool IsValidPosition(int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height && maze[x, y] == null;
	}

	private void Shuffle<T>(T[] array)
	{
		int n = array.Length;
		for (int i = 0; i < n; i++)
		{
			int r = i + random.Next(n - i);
			T temp = array[r];
			array[r] = array[i];
			array[i] = temp;
		}
	}
}

public class Tile
{
	public int Id { get; set; }
	public Vector2 Location { get; set; }
	public string TextureId { get; set; }
	public string HeightmapTextureId { get; set; }

	public override bool Equals(object obj)
	{
		var tile = obj as Tile;
		return Location.Equals(tile?.Location);
	}

	public override int GetHashCode()
	{
		return Location.GetHashCode();
	}
}

public class MapService
{
	public int Width = 768;
	public int Height = 512;
	public Tile[,] PathfindingTiles;

	public MapService(Tile[,] path)
	{
		PathfindingTiles = path;
	}

	public Tile GetTile(Vector2 location)
	{
		var gridWidth = PathfindingTiles.GetLength(0);
		var gridHeight = PathfindingTiles.GetLength(1);
		var x = (int)((location.X / Width) * gridWidth);
		var y = (int)((location.Y / Height) * gridHeight);
		x = Math.Max(0, Math.Min(x, gridWidth - 1));
		y = Math.Max(0, Math.Min(y, gridHeight - 1));
		return PathfindingTiles[x, y];
	}
}

public class PathfindingService
{
	private readonly MapService _map;

	public PathfindingService(MapService mapService)
	{
		_map = mapService;
	}

	public List<Tile> GetPath(Vector2 location, Vector2 target)
	{
		using var bmp = new Bitmap(1024, 768);
		using var gfx = Graphics.FromImage(bmp);
		var dc = new DumpContainer(bmp).Dump();
		var prevCol = Color.Red;
		
		// this is a bit hacky - just draw the obstacles
		for (int i = 0; i < 192; i++)
		{
			for (int j = 0; j < 128; j++)
			{
				var c = _map.PathfindingTiles[i, j];
				if (c == null)
					gfx.FillRectangle(new SolidBrush(Color.DarkSlateBlue), (int)i * 4, (int)j * 4, 4, 4);
			}
		}

		var visited = new Dictionary<int, Node<Tile>>();
		var potentials = new NodeQueue<float, Tile>(x => x.Value);

		var destination = _map.GetTile(target);
		var currentTile = _map.GetTile(location);

		var initialNode = CreateNode(currentTile, destination, null);
		potentials.Push(initialNode);

		bool found;
		Node<Tile> current;
		do
		{
			prevCol = GetNextColor(prevCol);
			
			current = potentials.Pop();

			var mask = GetMask(current.Item, _map.PathfindingTiles);
			foreach (var c in mask)
			{
				if (c == null || visited.ContainsKey(c.Id))
					continue;
				
				var node = CreateNode(c, destination, current);				
				visited[c.Id] = node;
				potentials.Push(node);

				gfx.DrawRectangle(new Pen(prevCol), (int)c.Location.X, (int)c.Location.Y, 4, 4);
			}
			dc.Refresh();
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
		
		// 1:07.105
		//var path = SimplifyPath(current, gfx);

		// 1:10.222
		var path = Prune(current).ToList();
		foreach (var p in path)
		{
			gfx.DrawRectangle(new Pen(Color.WhiteSmoke), (int)p.Location.X, (int)p.Location.Y, 4, 4);
		}
		
		path = SimplifyPath(path, gfx);
		gfx.DrawLines(new Pen(Color.WhiteSmoke), path.Select(x => new Point((int)x.Location.X, (int)x.Location.Y)).ToArray());
		dc.Refresh();

		return path;
	}

	bool direction = true;
	public Color GetNextColor(Color currentColor)
	{
		int red = currentColor.R;

		if (red == 255)
		{
			direction = false; // Reverse the direction
		}
		else if (red == 0)
		{
			direction = true; // Reset the direction to forward
		}
		
		if (direction)
		{
			red++; // Increment red when going forward
		}
		else
		{
			red--; // Decrement red when going backwards
		}

		return Color.FromArgb(red, currentColor.G, currentColor.B);
	}

	private Node<Tile> CreateNode(Tile tile, Tile destination, Node<Tile> current)
	{
		// todo: include entity speed at tile (type, gradient etc)
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

	private List<Tile> SimplifyPath(Node<Tile> tail, Graphics gfx)
	{
		int currentIndex = 0;
		var path = tail.ToList();
		int lastIndex = path.Count - 1;

		while (currentIndex < lastIndex - 1)
		{
			Tile current = path[currentIndex];
			Tile destination = path[lastIndex];

			if (!IsObstructed(current, destination, gfx))
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

	private List<Tile> SimplifyPath(List<Tile> path, Graphics gfx)
	{
		int currentIndex = 0;
		int lastIndex = path.Count - 1;

		var newPath = new List<Tile>(path);
		while (currentIndex < lastIndex - 1)
		{
			Tile current = newPath[currentIndex];
			Tile destination = newPath[lastIndex];

			if (!IsObstructed(current, destination, gfx))
			{
				// If there is a clear path from current to destination, remove all items between them
				newPath.RemoveRange(currentIndex + 1, lastIndex - currentIndex - 1);
				currentIndex++;
				lastIndex = newPath.Count - 1;
			}
			else
			{
				// If there is an obstruction, increment current and continue
				lastIndex--;
			}
		}
		return newPath;
	}

	public bool IsObstructed(Tile source, Tile destination, Graphics gfx)
	{
		var prevCol = Color.Red;
		
		int sourceX = (int)source.Location.X/4;
		int sourceY = (int)source.Location.Y/4;
		int destX = (int)destination.Location.X/4;
		int destY = (int)destination.Location.Y/4;

		// Perform raycasting from source to destination using the _grid variable
		// Assuming the grid has the same dimensions as the tile array

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
			prevCol = GetNextColor(prevCol);
			gfx.DrawRectangle(new Pen(prevCol), (int)x * 4, (int)y * 4, 4, 4);

			// Check if the current position encounters a null tile
			if (!IsNavigable(_map.PathfindingTiles[x, y]))
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

	private static Node<Tile> Prune(Node<Tile> tail)
	{
		const float threshold = 0.1f;
		var current = tail;

		while (current?.Previous?.Previous != null)
		{
			var temp = current.Previous.Previous;

			// if two points have the same slope, they are co-linear
			var currentSlope = (current.Previous.Item.Location.Y - current.Item.Location.Y) / (current.Previous.Item.Location.X - current.Item.Location.X);
			var previousSlope = (temp.Item.Location.Y - current.Previous.Item.Location.Y) / (temp.Item.Location.X - current.Previous.Item.Location.X);
			var isVertical = double.IsInfinity(currentSlope) && double.IsInfinity(previousSlope);

			// if all three points exist on the same line, remove the middle point
			if (Math.Abs(currentSlope - previousSlope) < threshold || isVertical)
				current.Previous = temp;
			else
				current = current.Previous;
		}
		return tail;
	}

	private List<Tile> GetMask(Tile current, Tile[,] tiles)
	{
		var gridWidth = tiles.GetLength(0);
		var gridHeight = tiles.GetLength(1);
		//var x = current.Id % gridWidth;      // Calculate the x value
		//var y = current.Id / gridHeight;     // Calculate the y value
		var x = (int) current.Location.X / 4;
		var y = (int) current.Location.Y / 4;

		var mask = new List<Tile>();

		int startX = Math.Max(x - 1, 0);
		int startY = Math.Max(y - 1, 0);
		int endX = Math.Min(x + 1, gridWidth - 1);
		int endY = Math.Min(y + 1, gridHeight - 1);

		// Iterate over neighboring tiles
		for (int i = startX; i <= endX; i++)
		{
			for (int j = startY; j <= endY; j++)
			{
				if (i == x && j == y) // dont include self in mask
					continue;
				// todo: 
				// if gradient to steep, terrain type is incompatible etc
				if (!IsNavigable(tiles[i, j]))
					continue;

				var tile = tiles[i, j];
				mask.Add(tile);
			}
		}

		return mask;
	}

	private bool IsNavigable(Tile tile)
	{
		// todo: 
	 	// if gradient to steep, terrain type is incompatible etc
		// entity.TileTypes, max gradient etc etc
		return tile != null;
	}

	//	private List<Tile> GetMask(Tile current, Tile[,] tiles)
	//	{
	//		var x = current.Id % _map.Width;      // Calculate the x value
	//		var y = current.Id / _map.Height;      // Calculate the y value
	//
	//		var mask = new List<Tile>();
	//
	//		var width = tiles.GetLength(0);     // Get the width of the tiles array
	//		var height = tiles.GetLength(1);    // Get the height of the tiles array
	//
	//		if (IsValidIndex(x - 1, y - 1, width, height))
	//			mask.Add(tiles[x - 1, y - 1]);
	//		if (IsValidIndex(x, y - 1, width, height))
	//			mask.Add(tiles[x, y - 1]);
	//		if (IsValidIndex(x + 1, y - 1, width, height))
	//			mask.Add(tiles[x + 1, y - 1]);
	//
	//		if (IsValidIndex(x - 1, y, width, height))
	//			mask.Add(tiles[x - 1, y]);
	//		if (IsValidIndex(x + 1, y, width, height))
	//			mask.Add(tiles[x + 1, y]);
	//
	//		if (IsValidIndex(x - 1, y + 1, width, height))
	//			mask.Add(tiles[x - 1, y + 1]);
	//		if (IsValidIndex(x, y + 1, width, height))
	//			mask.Add(tiles[x, y + 1]);
	//		if (IsValidIndex(x + 1, y + 1, width, height))
	//			mask.Add(tiles[x + 1, y + 1]);
	//
	//		return mask;
	//	}

	//private bool IsValidIndex(int x, int y, int width, int height)
	//{
	//	return x >= 0 && x < width && y >= 0 && y < height;
	//}

	//private List<Tile> GetMask(MovementComponent movementComponent, Tile current, Tile[,] tiles)
	//{
	//    var (currentX, currentY) = _map.GetCoordinates(current);
	//    var currentX = (int)current.Location.X / 4;
	//    var currentY = (int)current.Location.Y / 4;

	//    var results = new List<Tile>();

	//    movementComponent.MovementMask
	//    foreach (var (x, y) in movementMask)
	//    {
	//        if (currentX + x < 0 || currentY + y < 0 ||
	//            currentX + x > tiles.GetLength(0) - 1 ||
	//            currentY + y > tiles.GetLength(1) - 1)
	//            continue;

	//        var tile = tiles[currentX + x, currentY + y];
	//        if (movementComponent.Terrains.Contains(tile.Type))
	//            results.Add(tile);
	//    }
	//    return results;
	//}

	//// manhattan distance
	//public float GetDistance(Tile source, Tile destination) =>
	//    Math.Abs(source.Location.X - destination.Location.X) + Math.Abs(source.Location.Y - destination.Location.Y);

	// euclidean distance
	public float GetDistance(Tile source, Tile destination) =>
		Vector2.Distance(source.Location, destination.Location);
	
//	// octile distance
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

public class NodeQueue<TValue, TItem>
		where TValue : IComparable
{
	private readonly Func<Node<TItem>, TValue> _valueSelector;
	private readonly SortedList<TValue, Node<TItem>> _items;

	public class KeyComparer : IComparer<TValue>
	{
		public int Compare(TValue x, TValue y)
		{
			var result = x.CompareTo(y);

			return result == 0 ? 1 : result;
		}
	}

	public NodeQueue(Func<Node<TItem>, TValue> valueSelector)
	{
		_valueSelector = valueSelector;
		var keyComparer = new KeyComparer();
		_items = new SortedList<TValue, Node<TItem>>(keyComparer);
	}

	public void Push(Node<TItem> item)
	{
		var value = _valueSelector(item);
		_items.Add(value, item);
	}

	public Node<TItem> Pop()
	{
		var min = _items.First().Value;
		_items.RemoveAt(0);

		return min;
	}

	public bool Any() => _items.Any();
}

public class Node<T>
{
	public Node<T> Previous { get; set; }
	public T Item { get; set; }
	public float Value { get; set; }

	public Node(T source)
	{
		Item = source;
	}

	public List<T> ToList()
	{
		var results = new Stack<T>();
		var latest = this;
		while (latest != null)
		{
			results.Push(latest.Item);
			latest = latest.Previous;
		}
		return results.ToList();
	}
}