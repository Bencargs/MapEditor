using Common.Collision;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Handlers;
using MapEngine.ResourceLoading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace MapEngine.Factories
{
    public static class WeaponFactory
    {
        private static readonly Dictionary<string, WeaponComponent> _prototypes = new Dictionary<string, WeaponComponent>(StringComparer.OrdinalIgnoreCase);

        public static void LoadWeapons(string weaponsFilepath)
        {
            foreach (var file in Directory.GetFiles(weaponsFilepath, "*.json"))
            {
                var weapon = WeaponLoader.LoanWeaponDefinition(file);
                _prototypes.Add(weapon.WeaponType, weapon);
            }
        }

        public static bool TryGetWeapon(string type, out WeaponComponent weapon)
        {
            return _prototypes.TryGetValue(type, out weapon);
        }
    }
}
