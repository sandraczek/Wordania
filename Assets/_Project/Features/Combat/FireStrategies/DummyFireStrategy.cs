using System.Collections.Generic;
using UnityEngine;
using Wordania.Combat.Data;

namespace Wordania.Combat.FireStrategies
{
    public sealed class DummyFireStrategy : IWeaponFireStrategy
    {
        public WeaponType Type => WeaponType.Dummy;
        public int CalculateFireData(WeaponFireContext context, WeaponFireData data, List<ProjectileSpawnData> resultsBuffer)
        {
            resultsBuffer.Add(new ProjectileSpawnData
            {
                Position = context.position,
                Direction = context.direction,
                Data = data.Projectile,
                DamageMultiplier = context.damageMultiplier,
                SpeedMultiplier = 1f,
                LifetimeMultiplier = 1f,
                InstigatorId = context.instigatorId,
                TargetFactionMask = context.TargetFactionMask
            });

            return 1;
        }
    }
}