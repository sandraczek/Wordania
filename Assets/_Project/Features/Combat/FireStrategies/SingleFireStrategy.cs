
using System.Collections.Generic;
using UnityEngine;
using Wordania.Combat.Data;

namespace Wordania.Combat.FireStrategies
{
    public sealed class SingleFireStrategy : IWeaponFireStrategy
    {
        public WeaponType Type => WeaponType.Single;


        public int CalculateFireData(WeaponFireContext context, WeaponFireData data, List<ProjectileSpawnData> resultsBuffer)
        {
            var dir90 = new Vector2(-context.direction.y,context.direction.x);
            var position = context.position + data.BarrelWidth * (Random.value - 0.5f) * dir90;

            Vector2 direction = Quaternion.Euler(0f,0f, (Random.value - 0.5f) * data.Spread) * context.direction;

            resultsBuffer.Add(new ProjectileSpawnData
            {
                Position = position,
                Direction = direction,
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