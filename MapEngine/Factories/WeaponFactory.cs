using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.ResourceLoading;
using System.Collections.Generic;
using System.IO;

namespace MapEngine.Factories
{
    public static class WeaponFactory
    {
        private static readonly Dictionary<string, Entity> _prototypes = new Dictionary<string, Entity>();

        public static void Initialise(string weaponsFilepath)
        {
            foreach (var file in Directory.GetFiles(weaponsFilepath, "*.json"))
            {
                var weapon = WeaponLoader.LoanWeaponDefinition(file);
                var type = weapon.GetComponent<WeaponComponent>();
                _prototypes.Add(type.WeaponType, weapon);
            }
        }
    }
}
