using Common.Entities;

namespace MapEngine.Entities.Components
{
    public class WeaponComponent : IComponent
    {
        public ComponentType Type => ComponentType.Weapon;

        public string WeaponType { get; set; }
        public float Range { get; set; }
        public float Speed { get; set; }
        public int Damage { get; set; }
        public int MaxImpactForce { get; set; }
        public float ReloadTime { get; set; }

        public IComponent Clone()
        {
            return new WeaponComponent
            {
                WeaponType = WeaponType,
                Range = Range,
                Speed = Speed,
                Damage = Damage,
                MaxImpactForce = MaxImpactForce,
                ReloadTime = ReloadTime
            };
        }
    }
}
