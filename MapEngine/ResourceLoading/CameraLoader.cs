using Common;
using Newtonsoft.Json;
using System.IO;
using System.Numerics;

namespace MapEngine.ResourceLoading
{
    public static class CameraLoader
    {
        public static Camera LoadCamera(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic mapData = JsonConvert.DeserializeObject(json);

            var x = (int)mapData.Camera.X;
            var y = (int)mapData.Camera.Y;
            var z = (int)mapData.Camera.Z;
            var width = (int)mapData.Camera.Width;
            var height = (int)mapData.Camera.Height;

            var camera = new Camera
            {
                Location = new Vector3(x, y, z),
                Viewport = new Rectangle(x - (width / 2),
                                         y - (height / 2),
                                         width,
                                         height)
            };
            return camera;
        }
    }
}
