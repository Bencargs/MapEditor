﻿using Common.Entities;
using System;

namespace MapEngine.Entities.Components
{
    public class WeaponComponent : IComponent
    {
        public ComponentType Type => ComponentType.Weapon;

        public string TextureId { get; set; }
        public string WeaponType { get; set; }
        public float Range { get; set; } //todo: replace with a collider? (eg a cone)
        public float Speed { get; set; }
        public int Damage { get; set; }
        public int MaxImpactForce { get; set; }
        public float ReloadTime { get; set; }
        public DateTime LastFiredTime { get; set; }
        public int CollisionRadius { get; set; }

        public IComponent Clone()
        {
            return new WeaponComponent
            {
                TextureId = TextureId,
                WeaponType = WeaponType,
                Range = Range,
                Speed = Speed,
                Damage = Damage,
                MaxImpactForce = MaxImpactForce,
                ReloadTime = ReloadTime,
                CollisionRadius = CollisionRadius
            };
        }
    }
}
