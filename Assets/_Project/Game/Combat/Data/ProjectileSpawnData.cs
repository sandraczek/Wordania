using UnityEngine;
using Wordania.Identifiers;

namespace Wordania.Combat.Data
{
    /// <summary>Plain data (network-safe): the projectile asset is referenced by id and resolved by the factory.</summary>
    public struct ProjectileSpawnData
    {
        public Vector3 Position;
        public Vector2 Direction;
        public AssetId ProjectileId;
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public float LifetimeMultiplier;
        public InstanceId InstigatorId;
        public EntityFaction TargetFactionMask;
    }
}