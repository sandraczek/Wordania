using System;
using UnityEngine;
using Wordania.Gameplay.Inventory;

namespace Wordania.Gameplay.Enemies.Data
{
    [Serializable]
    public sealed class EnemyCombatData
    {
        [field: Header("Detection")]
        [field: SerializeField, Min(1f)] public float DetectionRadius { get; private set; } = 10f;
        [field: SerializeField, Min(1f)] public float LoseTargetRadius { get; private set; } = 15f;

        [field: Header("Attacking")]
        [field: SerializeField, Min(0.1f)] public float AttackRange { get; private set; } = 1.5f;
        [field: SerializeField, Min(0.1f)] public float AttackCooldown { get; private set; } = 2f;

        #if UNITY_EDITOR
        public void ForceValidTargetRadius()
        {
            LoseTargetRadius = DetectionRadius + 1f;
        }
        #endif
    }
}