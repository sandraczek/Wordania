using UnityEngine;
using Wordania.Core.Identifiers;
using Wordania.Features.Combat.Core;

namespace Wordania.Features.Combat.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Combat/Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        public string Name;
        public AssetId Id;
        public float FireRate;
        public WeaponType Type;
        public WeaponController Prefab;
        public ProjectileData Projectile;
    }

    public enum WeaponType
    {
        Single,
        Burst,
        Shotgun,
        Laser
    }
}