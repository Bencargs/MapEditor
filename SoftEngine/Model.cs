using SharpDX;
using System.Linq;

namespace SoftEngine
{
    public class Model
    {
        public Texture Texture { get; set; }
        public Vertex[] Vertices { get; set; }
        public Face[] Faces { get; set; }

        public Model(Common.IModel model, Common.ITexture texture)
        {
            Texture = new Texture(texture);
            Vertices = model.Vertices.Select(x => new Vertex
            {
                Normal = ToVector(x.Normal),
                Coordinates = ToVector(x.Coordinates),
                WorldCoordinates = ToVector(x.WorldCoordinates),
                TextureCoordinates = ToVector(x.TextureCoordinates)
            }).ToArray();
            Faces = model.Faces.Select(x => new Face
            {
                VertexA = x.VertexA,
                VertexB = x.VertexB,
                VertexC = x.VertexC,
                Normal = ToVector(x.Normal)
            }).ToArray();
        }

        private static Vector3 ToVector(System.Numerics.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        private static Vector2 ToVector(System.Numerics.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
