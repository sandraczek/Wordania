using System;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Core;

namespace Wordania.Features.Combat.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Combat/Weapon")]
    public sealed class WeaponData : DataAsset
    {
        [Space]
        public string Name;
        public WeaponType Type;
        public WeaponController Prefab;

        public WeaponFireData FireData;

        public Sprite Icon;
    }

    [Serializable]
    public sealed class WeaponFireData
    {
        [Min(0.001f)] public float FireRate = 0.3f;
        public float Spread = 0f;
        public float BarrelWidth = 0f;
        public int ProjectileCount = 1;
        public float SpeedMultRange = 0f;
        public float LifetimeMultRange = 0f;
        public ProjectileData Projectile;
    }

    public enum WeaponType
    {
        Dummy,
        Single,
        ConeSpread,
        Beam
    }
}