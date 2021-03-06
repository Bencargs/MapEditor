﻿using MapEngine.Entities.Components;
using Newtonsoft.Json;
using System.IO;

namespace MapEngine.ResourceLoading
{
    public static class WeaponLoader
    {
        public static WeaponComponent LoanWeaponDefinition(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic weaponData = JsonConvert.DeserializeObject(json);

            var weapon = new WeaponComponent
            {
                TextureId = weaponData.TextureId,
                WeaponType = weaponData.Type,
                Range = weaponData.Range,
                Speed = weaponData.Speed,
                ReloadTime = weaponData.ReloadTime,
                MaxImpactForce = weaponData.MaxImpactForce,
                Damage = weaponData.Damage,
                CollisionRadius = weaponData.CollisionRadius
            };

            return weapon;
        }
    }
}
