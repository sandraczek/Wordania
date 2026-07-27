using UnityEngine;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Combat.Data
{
    public struct ProjectileSpawnData
    {
        public Vector3 Position;
        public Vector2 Direction;
        public ProjectileData Data;
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public float LifetimeMultiplier;
        public InstanceId InstigatorId;
        public EntityFaction TargetFactionMask;
    }
}