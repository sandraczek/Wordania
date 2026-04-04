using System.Collections.Generic;
using UnityEngine;
using Wordania.Core;
using Wordania.Features.Combat.Data;

namespace Wordania.Features.Combat.FireStrategies
{
    public sealed class StraightFireStrategy : IWeaponFireStrategy
    {
        public WeaponType Type => WeaponType.Single;
        public int CalculateFireData(WeaponFireContext context, ProjectileData data, List<ProjectileSpawnData> resultsBuffer)
        {
            resultsBuffer.Add(new ProjectileSpawnData
            {
                Position = context.position,
                Direction = context.direction,
                Data = data,
                DamageMultiplier = context.damageMultiplier,
                InstigatorId = context.instigatorId,
                TargetFactionMask = context.TargetFactionMask
            });

            return 1;
        }
    }
}