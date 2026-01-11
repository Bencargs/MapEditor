using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Common;
using Newtonsoft.Json;

namespace MapEngine.ResourceLoading
{
    public static class CursorLoader
    {
        public static CursorDefinition LoadCursor(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic data = JsonConvert.DeserializeObject(json);
            
            var cursorName = (string)data.Name;
            return new CursorDefinition
            {
                Name = cursorName,
                Hotspot = new Vector2((int)data.Hotspot.X, (int)data.Hotspot.Y)
            };
        }
    }
}