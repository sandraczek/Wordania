
using System.Collections.Generic;
using UnityEngine;
using Wordania.Combat.Data;

namespace Wordania.Combat.FireStrategies
{
    public sealed class ConeSpreadFireStrategy : IWeaponFireStrategy
    {
        public WeaponType Type => WeaponType.ConeSpread; 

        public int CalculateFireData(WeaponFireContext context, WeaponFireData data, List<ProjectileSpawnData> resultsBuffer)
        {
            var dir90 = new Vector2(-context.direction.y,context.direction.x);

            for (int i = 0;i<data.ProjectileCount;i++){

                var position = context.position + data.BarrelWidth * (Random.value - 0.5f) * dir90;
                Vector2 direction = Quaternion.Euler(0f,0f, (Random.value - 0.5f) * data.Spread) * context.direction;
                float speedMult = 1f + (Random.value - 0.5f) * data.SpeedMultRange;
                float lifeTimeMult = 1f + (Random.value - 0.5f) * data.LifetimeMultRange;

                resultsBuffer.Add(new ProjectileSpawnData
                {
                    Position = position,
                    Direction = direction,
                    Data = data.Projectile,
                    DamageMultiplier = context.damageMultiplier,
                    SpeedMultiplier = speedMult,
                    LifetimeMultiplier = lifeTimeMult,
                    InstigatorId = context.instigatorId,
                    TargetFactionMask = context.TargetFactionMask
                });
            }

            return data.ProjectileCount;
        }
    }
}