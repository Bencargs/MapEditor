<Query Kind="Program">
  <Reference Relative="..\packages\Newtonsoft.Json.11.0.2\lib\net45\Newtonsoft.Json.dll">C:\Source\MapEditor\packages\Newtonsoft.Json.11.0.2\lib\net45\Newtonsoft.Json.dll</Reference>
</Query>

public class Thing
{
	public int Id {get; set;}
	public string Filename {get;set;}
}

void Main()
{
	var map = new MapData
	{
		Width = 768,
		Height = 512,
		Heightmap = "textures/heightmap.png",
		Textures = new[]
		{
			new Thing {Id = 1, Filename = "animations/water.gif"}
		},
		Tiles = new[]
		{
			new Tile
			{
				Id	= 0,
				Location = new Point(0, 0),
				TextureId = 1,
				Type = TerrainType.Water
			},
			new Tile
			{
				Id  = 0,
				Location = new Point(256, 0),
				TextureId = 1,
				Type = TerrainType.Water
			},
			new Tile
			{
				Id  = 0,
				Location = new Point(0, 256),
				TextureId = 1,
				Type = TerrainType.Water
			},
			new Tile
			{
				Id  = 0,
				Location = new Point(256, 256),
				TextureId = 1,
				Type = TerrainType.Water
			},
			new Tile
			{
				Id  = 0,
				Location = new Point(512, 0),
				TextureId = 1,
				Type = TerrainType.Water
			},
			new Tile
			{
				Id  = 0,
				Location = new Point(512, 256),
				TextureId = 1,
				Type = TerrainType.Water
			},
		}
	};
	
	var json = Newtonsoft.Json.JsonConvert.SerializeObject(map, Newtonsoft.Json.Formatting.Indented);
	File.WriteAllText(@"C:\Source\MapEditor\Sandbox\TestMap1.json", json);
}

public class MapData
{
	public int Width {get; set;}
	public int Height {get; set;}
	public string Heightmap {get; set;}
	public Thing[] Textures { get; set; }
	public Tile[] Tiles { get; set; }
}

public class Point
{
	public int X {get; set;}
	public int Y {get; set;}
	
	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}
	
}

public class Tile
{
	public int Id { get; set;}
	public Point Location {get; set;}
	public int TextureId {get; set;}
	public TerrainType Type {get; set;}
}

public enum TerrainType
{
	Empty = 0,
	Water,
	Land,
}