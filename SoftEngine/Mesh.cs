using SharpDX;
using System.Threading.Tasks;

namespace SoftEngine
{
    public class Mesh
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Texture Texture { get; set; }

        public Mesh(string name, int verticiesCount, int faceCount)
        {
            Vertices = new Vertex[verticiesCount];
            Faces = new Face[faceCount];
            Name = name;
        }

        public void Rotate(float x, float y, float z)
        {
            Rotation = new Vector3(x, y, z);
        }

        public void Translate(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public void ComputeFacesNormals()
        {
            Parallel.For(0, Faces.Length, faceIndex =>
            {
                var face = Faces[faceIndex];
                var vertexA = Vertices[face.A];
                var vertexB = Vertices[face.B];
                var vertexC = Vertices[face.C];

                Faces[faceIndex].Normal = (vertexA.Normal + vertexB.Normal + vertexC.Normal) / 3.0f;
                Faces[faceIndex].Normal.Normalize();
            });
        }
    }
}
