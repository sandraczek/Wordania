using System;
using UnityEngine;

namespace Wordania.Enemies.Data
{
    [Serializable]
    public sealed class EnemyStatsData
    {
        [field: SerializeField, Min(1f)] public float MaxHealth { get; private set; } = 100f;
    }
}