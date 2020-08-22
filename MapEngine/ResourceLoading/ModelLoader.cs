using Newtonsoft.Json;
using System.IO;
using System.Numerics;

namespace MapEngine.ResourceLoading
{
    public static class ModelLoader
    {
        public static Model LoadModel(string modelFilename)
        {
            var data = "";
            using (var reader = File.OpenText(modelFilename))
            {
                data = reader.ReadToEnd();
            }
            dynamic jsonObject = JsonConvert.DeserializeObject(data);

            //var materials = new Dictionary<string, Material>();
            //for (var materialIndex = 0; materialIndex < jsonObject.materials.Count; materialIndex++)
            //{
            //    var material = new Material
            //    {
            //        Name = jsonObject.materials[materialIndex].name.Value,
            //        ID = jsonObject.materials[materialIndex].id.Value
            //    };
            //    if (jsonObject.materials[materialIndex].diffuseTexture != null)
            //        material.DiffuseTextureName = jsonObject.materials[materialIndex].diffuseTexture.name.Value;

            //    materials.Add(material.ID, material);
            //}

            Model model;
            //for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            var meshIndex = 0;
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                // Faces
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array  by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                // number of faces is logically the size of the array divided by 3 (A, B, C)
                var facesCount = indicesArray.Count / 3;
                model = new Model(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                // Filling the Vertices array of our mesh first
                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value;
                    var y = (float)verticesArray[index * verticesStep + 1].Value;
                    var z = (float)verticesArray[index * verticesStep + 2].Value;
                    // Loading the vertex normal exported by Blender
                    var nx = (float)verticesArray[index * verticesStep + 3].Value;
                    var ny = (float)verticesArray[index * verticesStep + 4].Value;
                    var nz = (float)verticesArray[index * verticesStep + 5].Value;

                    model.Vertices[index] = new Vertex
                    {
                        Coordinates = new Vector3(x, y, z),
                        Normal = new Vector3(nx, ny, nz)
                    };

                    if (uvCount > 0)
                    {
                        // Loading the texture coordinates
                        float u = (float)verticesArray[index * verticesStep + 6].Value;
                        float v = (float)verticesArray[index * verticesStep + 7].Value;
                        model.Vertices[index].TextureCoordinates = new Vector2(u, v);
                    }
                }

                // Then filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    model.Faces[index] = new Face { VertexA = a, VertexB = b, VertexC = c };
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                model.Location = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);

                //if (uvCount > 0)
                //{
                //    // Texture
                //    //var meshTextureID = jsonObject.meshes[meshIndex].materialId.Value;
                //    //var meshTextureName = materials[meshTextureID].DiffuseTextureName;
                //}

                model.ComputeFaceNormals();
            }

            // Todo - bug? seems to load objects in facing away from camera
            // equivilent of rotating by 1 radian
            model.Rotation = new Vector3(4.7f, 6.3f, 0);

            return model;
        }
    }
}
