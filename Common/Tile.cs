namespace Common
{
	public class Tile
	{
		public int Id { get; set; }
		public Point Location { get; set; }
		public int TextureId { get; set; }
		public TerrainType Type { get; set; }
	}
}
