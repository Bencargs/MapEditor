<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>System.Numerics</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
	var rng = new Random();
	var height = 20;
	var width = 20;
	var tiles = new List<Tile>(width * height);
	
	var tileId = 0;
	var tileSize = 24;
	for (int y = 0; y < height; y++)
	{
		for (int x = 0; x < width; x++)
		{
			var terrain  = (TerrainType)rng.Next(3);
			terrain = terrain == TerrainType.Impassible ? TerrainType.Ground : terrain; // lets just have ground and water for now
			tiles.Add(new Tile
			{
				Id = tileId++,
				Size = tileSize,
				Type = terrain,
				TextureId = GetTextureId(terrain),
				Location = new Vector2(x * tileSize, y * tileSize)
			});
		}
	}
	
	var map = new {
		Width = tileSize * width,
		Height = tileSize * height,
		Description = "Pathfinding Maze Test",
		TileWidth = width,
		TileHeight = height,
		Camera = new {
			X = 0,
			Y = 0,
			Z = 10,
			Width = 768,
			Height = 512
		},
		Teams = new[] { 
			new {
				Id = 0,
				Name = "Player"
			}
		},
		Units = new[] {
			new {
				Id = 0,
				Team = 0,
				Type = "MobileDummy",
				Location = new {
					X = 300,
					Y = 50, 
					Z = 2
				},
				Movement = new {
					FacingAngle = 0,
					Velocity = new {
						X=0,
						Y=0,
					},
					Steering = new {
						X=0,
						Y=0,
				},
				Destinations = new[] {
					new {
							MovementMode = "Seek",
							Destination = new {
								X=50,
								Y=50
							}
						}
					}
				}
			}
		},
		Tiles = tiles
	};

	var json = JsonConvert.SerializeObject(map, Newtonsoft.Json.Formatting.Indented);
	File.WriteAllText(@"C:\Source\MapEditor\Sandbox\Maze.json", json);
}

//private string GetTextureId(TerrainType terrain) =>
//	terrain switch
//	{
//		TerrainType.Impassible => "Black",
//		TerrainType.Ground => "Green",
//		TerrainType.Water => "Blue",
//		_ => "Unknown"
//	};
private string GetTextureId(TerrainType terrain)
{
	switch(terrain)
	{
		case TerrainType.Impassible:
			return "Black";
		case TerrainType.Ground:
			return "Green";
		case TerrainType.Water:
			return "Blue";
		default:
			return "Unknown";
	}
}

public class Tile
{
	public int Id {get; set;}
	public int Size {get; set;}
	public Vector2 Location {get; set;}
	public string TextureId {get; set;}
	public TerrainType Type {get; set;}
}

public enum TerrainType
{
	Impassible,
	Water,
	Ground
}
