using Common.Entities;
using MapEngine.Entities.Components;
using Newtonsoft.Json;
using System.IO;

namespace MapEngine.ResourceLoading
{
    public static class WeaponLoader
    {
        public static Entity LoanWeaponDefinition(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic weaponData = JsonConvert.DeserializeObject(json);

            var entity = new Entity();

            entity.AddComponent(new LocationComponent());

            entity.AddComponent(new MovementComponent());

            entity.AddComponent(new WeaponComponent
            {
                WeaponType = weaponData.Type,
                Range = weaponData.Range,
                Speed = weaponData.Speed,
                ReloadTime = weaponData.ReloadTime,
                MaxImpactForce = weaponData.MaxImpactForce,
                Damage = weaponData.Damage
            });

            entity.AddComponent(new ImageComponent
            {
                TextureId = weaponData.TextureId
            });

            return entity;
        }
    }
}
