<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Numerics.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
	var tileId = 0;
	var tileSize = 4;
	var tiles = new List<Tile>(127 * 130);
	var filename = @"C:\Source\MapEditor\Sandbox\tilemap.png";
	using (var bmp = new Bitmap(filename))
	{
		for (int x = 0; x < bmp.Width; x+=tileSize)
		{
			for (int y = 0; y < bmp.Height; y+=tileSize)
			{
				var area = new Rectangle(x, y, tileSize, tileSize);
				var terrain = GetTileType(bmp, x, y, tileSize);
				var tile = new Tile
				{
					Id = tileId++,
					Size = tileSize,
					Type = terrain,
					TextureId = terrain == TerrainType.Water ? "Blue" : "Green",
					Location = new Vector2(x, y)
				};
				tiles.Add(tile);
			}
		}
	}

	var map = new
	{
		Width = tileSize * 127,
		Height = tileSize * 130,
		Description = "Wave Test",
		TileWidth = 127,
		TileHeight = 130,
		Camera = new
		{
			X = 0,
			Y = 0,
			Z = 10,
			Width = 768,
			Height = 512
		},
		Units = new object[0],
		Teams = new[] {
			new {
				Id = 0,
				Name = "Player"
			}
		},
		Tiles = tiles
	};

	var json = JsonConvert.SerializeObject(map, Newtonsoft.Json.Formatting.Indented);
	File.WriteAllText(@"C:\Source\MapEditor\Sandbox\Waves.json", json);
}

private TerrainType GetTileType(Bitmap img, int startX, int startY, int tileSize)
{
	var blue = 0;
	var total = 0;
	for (int x = startX; x < startX + tileSize; x++)
	{
		for (int y = startY; y < startY + tileSize; y++)
		{
			if (x > img.Width - 1 || y > img.Height - 1)
				continue;

			var pixel = img.GetPixel(x, y);
			if (pixel == Color.FromArgb(255, 0, 0, 255))
				blue++;
			total++;
		}
	}
	
	var isWater = ((float)blue / total) > 0.5;
	return isWater ? TerrainType.Water : TerrainType.Land;
}

public class Tile
{
	public int Id { get; set; }
	public int Size {get; set;}
	public Vector2 Location { get; set; }
	public string TextureId { get; set; }
	public TerrainType Type { get; set; }
}

public enum TerrainType
{
	Empty = 0,
	Water,
	Land,
}