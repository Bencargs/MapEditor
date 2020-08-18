using MapEngine.ResourceLoading;
using System;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Factories
{
    public class ModelFactory
    {
        public static Dictionary<string, Model> _models = new Dictionary<string, Model>(StringComparer.OrdinalIgnoreCase);

        public static void LoadModel(string filepath)
        {
            foreach (var file in Directory.GetFiles(filepath, "*.babylon"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var model = ModelLoader.LoadModel(file);
                _models.Add(name, model);
            }
        }

        public static bool TryGetModel(string modelId, out Model model)
        {
            return _models.TryGetValue(modelId, out model);
        }
    }
}
