using Wordania.Combat.Data;
using UnityEngine;
using System.Collections.Generic;

namespace Wordania.Combat.FireStrategies
{
    public interface IWeaponFireStrategy
    {
        WeaponType Type { get; }
        public int CalculateFireData(WeaponFireContext context, WeaponFireData weaponData, List<ProjectileSpawnData> resultsBuffer);
    }
}