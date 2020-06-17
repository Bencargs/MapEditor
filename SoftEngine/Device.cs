using Common;
using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SoftEngine
{
    public class Device
    {
        private readonly object[] _lockBuffer;
        private readonly byte[] _backBuffer;
        private readonly float[] _depthBuffer;
        private readonly IImage _bmp;
        private readonly int _width;
        private readonly int _height;

        public Device(IImage bmp)
        {
            _bmp = bmp;
            _width = bmp.Width;
            _height = bmp.Height;
            var size = _width * _height;

            // the back buffer size is equal to the number of pixels to draw
            // on screen (width*height) * 4 (R,G,B & Alpha values). 
            _backBuffer = new byte[size * 4];
            _depthBuffer = new float[size];
            _lockBuffer = Enumerable.Repeat(new object(), size).ToArray();
        }

        // This method is called to clear the back buffer with a specific colour
        private void Clear(byte r, byte g, byte b, byte a)
        {
            //Array.Clear(_backBuffer, 0, _backBuffer.Length);
            // Clearing Back Buffer
            for (var index = 0; index < _backBuffer.Length; index += 4)
            {
                // BGRA is used by Windows instead by RGBA in HTML5
                _backBuffer[index] = b;
                _backBuffer[index + 1] = g;
                _backBuffer[index + 2] = r;
                _backBuffer[index + 3] = a;
            }

            // Clearing Depth Buffer
            for (var index = 0; index < _depthBuffer.Length; index++)
            {
                _depthBuffer[index] = float.MaxValue;
            }
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        private void PutPixel(int x, int y, float z, Color4 colour)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = (x + y * _width);
            var index4 = index * 4;

            lock (_lockBuffer[index])
            {
                if (_depthBuffer[index] < z)
                {
                    return; // Discard
                }

                _depthBuffer[index] = z;

                _backBuffer[index4] = (byte)(colour.Blue * 255);
                _backBuffer[index4 + 1] = (byte)(colour.Green * 255);
                _backBuffer[index4 + 2] = (byte)(colour.Red * 255);
                _backBuffer[index4 + 3] = (byte)(colour.Alpha * 255);
            }
        }

        // Project takes some 3D coordinates and transform them
        // in 2D coordinates using the transformation matrix
        // It also transform the same coordinates and the norma to the vertex 
        // in the 3D world
        private Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            // transforming the coordinates into 2D space
            var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transMat);
            // transforming the coordinates & the normal to the vertex in the 3D world
            var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, world);
            var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, world);

            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point2d.X * _width + _width / 2.0f;
            var y = -point2d.Y * _height + _height / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point2d.Z),
                Normal = normal3dWorld,
                WorldCoordinates = point3dWorld,
                TextureCoordinates = vertex.TextureCoordinates
            };
        }

        // Compute the cosine of the angle between the light vector and the normal vector
        // Returns a value between 0 and 1
        private float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }

        // Once everything is ready, we can swap buffers 
        private void Flush()
        {
            _bmp.Draw(_backBuffer);
        }

        // DrawPoint calls PutPixel but does the clipping operation before
        private void DrawPoint(Vector3 point, Color4 colour)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < _width && point.Y < _height)
            {
                // Drawing a point
                PutPixel((int)point.X, (int)point.Y, point.Z, colour);
            }
        }

        // Clamping values to keep them between 0 and 1
        private float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        private float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        private void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color4 colour, Texture texture)
        {
            Vector3 pa = va.Coordinates;
            Vector3 pb = vb.Coordinates;
            Vector3 pc = vc.Coordinates;
            Vector3 pd = vd.Coordinates;

            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            // Interpolating normals on Y
            var snl = Interpolate(data.ndotla, data.ndotlb, gradient1);
            var enl = Interpolate(data.ndotlc, data.ndotld, gradient2);

            // Interpolating texture coordinates on Y
            var su = Interpolate(data.ua, data.ub, gradient1);
            var eu = Interpolate(data.uc, data.ud, gradient2);
            var sv = Interpolate(data.va, data.vb, gradient1);
            var ev = Interpolate(data.vc, data.vd, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                // Interpolating Z, normal and texture coordinates on X
                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                var u = Interpolate(su, eu, gradient);
                var v = Interpolate(sv, ev, gradient);

                Color4 textureColour;

                if (texture != null)
                    textureColour = texture.Map(u, v);
                else
                    textureColour = new Color4(1, 1, 1, 1);

                // changing the native colour value using the cosine of the angle
                // between the light vector and the normal vector
                // and the texture colour
                DrawPoint(new Vector3(x, data.currentY, z), colour * ndotl * textureColour);
            }
        }

        private void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Color4 colour, Texture texture)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            Vector3 p1 = v1.Coordinates;
            Vector3 p2 = v2.Coordinates;
            Vector3 p3 = v3.Coordinates;

            // Light position 
            Vector3 lightPos = new Vector3(0, 10, 10);

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the colour
            float nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal, lightPos);
            float nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal, lightPos);
            float nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal, lightPos);

            // computing lines' directions
            // http://en.wikipedia.org/wiki/Slope
            // Computing slopes
            float dP1P2 = p2.Y - p1.Y > 0 ? (p2.X - p1.X) / (p2.Y - p1.Y) : 0;

            float dP1P3 = p3.Y - p1.Y > 0 ? (p3.X - p1.X) / (p3.Y - p1.Y) : 0;

            if (dP1P2 > dP1P3)
            {
                // Where triangles are like:
                // P1
                // - -
                // -   - P2
                // -  -
                // P3
                DrawRightTriangle(
                    colour, texture,
                    p1, p2, p3,
                    v1, v2, v3,
                    nl1, nl2, nl3);
            }
            else
            {
                // Where triangles are like:
                //       P1
                //     -  -
                // P2 -   - 
                //     -  -
                //       P3
                DrawLeftTriangle(
                    colour, texture,
                    p1, p2, p3,
                    v1, v2, v3,
                    nl1, nl2, nl3);
            }
        }

        private void DrawRightTriangle(
            Color4 colour, Texture texture,
            Vector3 p1, Vector3 p2, Vector3 p3,
            Vertex v1, Vertex v2, Vertex v3,
            float nl1, float nl2, float nl3)
        {
            var data = new ScanLineData();
            for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
            {
                data.currentY = y;

                if (y < p2.Y)
                {
                    data.ndotla = nl1;
                    data.ndotlb = nl3;
                    data.ndotlc = nl1;
                    data.ndotld = nl2;

                    data.ua = v1.TextureCoordinates.X;
                    data.ub = v3.TextureCoordinates.X;
                    data.uc = v1.TextureCoordinates.X;
                    data.ud = v2.TextureCoordinates.X;

                    data.va = v1.TextureCoordinates.Y;
                    data.vb = v3.TextureCoordinates.Y;
                    data.vc = v1.TextureCoordinates.Y;
                    data.vd = v2.TextureCoordinates.Y;

                    ProcessScanLine(data, v1, v3, v1, v2, colour, texture);
                }
                else
                {
                    data.ndotla = nl1;
                    data.ndotlb = nl3;
                    data.ndotlc = nl2;
                    data.ndotld = nl3;

                    data.ua = v1.TextureCoordinates.X;
                    data.ub = v3.TextureCoordinates.X;
                    data.uc = v2.TextureCoordinates.X;
                    data.ud = v3.TextureCoordinates.X;

                    data.va = v1.TextureCoordinates.Y;
                    data.vb = v3.TextureCoordinates.Y;
                    data.vc = v2.TextureCoordinates.Y;
                    data.vd = v3.TextureCoordinates.Y;

                    ProcessScanLine(data, v1, v3, v2, v3, colour, texture);
                }
            }
        }

        private void DrawLeftTriangle(
            Color4 colour, Texture texture,
            Vector3 p1, Vector3 p2, Vector3 p3,
            Vertex v1, Vertex v2, Vertex v3,
            float nl1, float nl2, float nl3)
        {
            var data = new ScanLineData();
            for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
            {
                data.currentY = y;

                if (y < p2.Y)
                {
                    data.ndotla = nl1;
                    data.ndotlb = nl2;
                    data.ndotlc = nl1;
                    data.ndotld = nl3;

                    data.ua = v1.TextureCoordinates.X;
                    data.ub = v2.TextureCoordinates.X;
                    data.uc = v1.TextureCoordinates.X;
                    data.ud = v3.TextureCoordinates.X;

                    data.va = v1.TextureCoordinates.Y;
                    data.vb = v2.TextureCoordinates.Y;
                    data.vc = v1.TextureCoordinates.Y;
                    data.vd = v3.TextureCoordinates.Y;

                    ProcessScanLine(data, v1, v2, v1, v3, colour, texture);
                }
                else
                {
                    data.ndotla = nl2;
                    data.ndotlb = nl3;
                    data.ndotlc = nl1;
                    data.ndotld = nl3;

                    data.ua = v2.TextureCoordinates.X;
                    data.ub = v3.TextureCoordinates.X;
                    data.uc = v1.TextureCoordinates.X;
                    data.ud = v3.TextureCoordinates.X;

                    data.va = v2.TextureCoordinates.Y;
                    data.vb = v3.TextureCoordinates.Y;
                    data.vc = v1.TextureCoordinates.Y;
                    data.vd = v3.TextureCoordinates.Y;

                    ProcessScanLine(data, v2, v3, v1, v3, colour, texture);
                }
            }
        }

        // The main method of the engine that re-compute each vertex projection
        // during each frame
        public void Render(/*Camera camera, */params Mesh[] meshes)
        {
            var camera = new Camera
            {
                Position = new Vector3(0, 0, 50),
                Target = Vector3.Zero
            };

            Clear(255, 255, 255, 0);

            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovLH(0.78f,
                                                           (float)_width / _height,
                                                           0.01f, 1.0f);

            foreach (var mesh in meshes)
            {
                // Beware to apply rotation before translation 
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) *
                                  Matrix.Translation(mesh.Position);

                var worldView = worldMatrix * viewMatrix;
                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                Parallel.For(0, mesh.Faces.Length, faceIndex =>
                {
                    var face = mesh.Faces[faceIndex];

                    // Face-back culling - !!! this is buggy (uses isometric not persepctive)
                    var transformedNormal = Vector3.TransformNormal(face.Normal, worldView);
                    if (transformedNormal.Z >= 0)
                    {
                        return;
                    }

                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    var pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    var pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    var colour = 1.0f;
                    DrawTriangle(pixelA, pixelB, pixelC, new Color4(colour, colour, colour, 1), mesh.Texture);
                });
            }

            Flush();
        }
    }
}
