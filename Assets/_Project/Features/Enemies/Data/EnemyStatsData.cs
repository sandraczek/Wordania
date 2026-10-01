using System;
using UnityEngine;

namespace Wordania.Enemies.Data
{
    [Serializable]
    public sealed class EnemyStatsData
    {
        [field: SerializeField, Min(1f)] public float MaxHealth { get; private set; } = 100f;
        [field: SerializeField, Min(0f)] public float TouchDamage { get; private set; } = 10f;
        [field: SerializeField, Range(0f, 1f)] public float KnockbackResistance { get; private set; } = 0f;
    }
}