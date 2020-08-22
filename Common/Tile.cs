using System.Numerics;

namespace Common
{
	public class Tile
	{
		public int Id { get; set; }
		public int Size { get; set; }
		public Vector2 Location { get; set; }
		public string TextureId { get; set; }
		public TerrainType Type { get; set; }
	}
}
