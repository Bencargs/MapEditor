using System.Numerics;

namespace Common
{
    public interface IModel
    {
        string ModelId { get; set; }
        IVertex[] Vertices { get; set; }
        IFace[] Faces { get; set; }
        Vector3 Location { get; set; }
        Vector3 Rotation { get; set; }

        void ComputeFaceNormals();
    }

    public interface IFace
    {
        int VertexA { get; set; }
        int VertexB { get; set; }
        int VertexC { get; set; }
        Vector3 Normal { get; set; }
    }

    public interface IVertex
    {
        Vector3 Normal { get; set; }
        Vector3 Coordinates { get; set; }
        Vector3 WorldCoordinates { get; set; }
        Vector2 TextureCoordinates { get; set; }
    }
}
