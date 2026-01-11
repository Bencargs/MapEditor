using System.Numerics;

namespace Common
{
	public class Tile
	{
		public int Id { get; set; }
		public Vector2 Location { get; set; }
		public string TextureId { get; set; }
		public string SurfaceTextureId { get; set; } = null;
		public string SubSurfaceTextureId { get; set; } = null;
		public TerrainType Type { get; set; }
		public string HeightmapTextureId { get; set; }
        public Vector3 Normal { get; set; }

        // todo: no idea what black magic i've cast here - probably wrong
        //public float GetGradient() => 1 - Vector3.Dot(Normal, Vector3.UnitZ);
        public float GetGradient() => 1 - Normal.Z;

        public override bool Equals(object obj)
        {
            var tile = obj as Tile;
            return Location == tile?.Location;
        }

        public override int GetHashCode()
        {
            return Location.GetHashCode();
        }
    }
}
