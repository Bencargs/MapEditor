using Common;
using System.Numerics;
using System.Threading.Tasks;

namespace MapEngine
{
    public class Vertex : IVertex
    {
        public Vector3 Normal { get; set; }
        public Vector3 Coordinates { get; set; }
        public Vector3 WorldCoordinates { get; set; }
        public Vector2 TextureCoordinates { get; set; }
    }

    public class Face : IFace
    {
        public int VertexA { get; set; }
        public int VertexB { get; set; }
        public int VertexC { get; set; }
        public Vector3 Normal { get; set; }
    }

    public class Model : IModel
    {
        public string ModelId { get; set; }
        public IVertex[] Vertices { get; set; }
        public IFace[] Faces { get; set; }
        public Vector3 Location { get; set; }
        public Vector3 Rotation { get; set; }

        public Model()
        { }

        public Model(string modelId, int verticesCount, int facesCount)
        {
            ModelId = modelId;
            Vertices = new IVertex[verticesCount];
            Faces = new IFace[facesCount];
        }

        public void ComputeFaceNormals()
        {
            Parallel.For(0, Faces.Length, faceIndex =>
            {
                var face = Faces[faceIndex];
                var vertexA = Vertices[face.VertexA];
                var vertexB = Vertices[face.VertexB];
                var vertexC = Vertices[face.VertexC];

                Faces[faceIndex].Normal = (vertexA.Normal + vertexB.Normal + vertexC.Normal) / 3.0f;
                Faces[faceIndex].Normal = Faces[faceIndex].Normal.Normalize();
            });
        }
    }
}
