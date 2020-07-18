using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.ResourceLoading;
using System;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Factories
{
    public class UnitFactory
    {
        private static Dictionary<string, Entity> _prototypes = new Dictionary<string, Entity>(StringComparer.OrdinalIgnoreCase);

        public static void LoadUnits(string filepath)
        {
            foreach (var file in Directory.GetFiles(filepath, "*.json"))
            {
                var unit = UnitLoader.LoadUnitDefinition(file);
                var type = unit.GetComponent<UnitComponent>();
                _prototypes.Add(type.UnitType, unit);
            }
        }

        public static bool TryGetUnit(string type, out Entity unit)
        {
            return _prototypes.TryGetValue(type, out unit);
        }
    }
}
