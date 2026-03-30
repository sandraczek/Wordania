using System;
using UnityEngine;
using Wordania.Core.Combat;
using Wordania.Features.Inventory;

namespace Wordania.Features.Enemies.Data
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
        [SerializeField] public float ContactDamage = 26f;
        [SerializeField] public Vector2 Knockback = new(15f,7f);
        [SerializeField] public DamageType DamageType = DamageType.Physical;
        [SerializeField] public HealthChangeSource DamageSource = HealthChangeSource.Generic;

        [Header("Damage")]
        [SerializeField] public float HitStunDuration = 0.2f;
        [SerializeField] public float InvincibilityDuration = 0.2f;
        [SerializeField] public float GeneralResistance = 0f;
        [SerializeField] public float PhysicalResistance = 0f;
        [SerializeField] public float MagicalResistance = 0f;
        [SerializeField] public float EnvironmentalResistance = 0f;
        [SerializeField] public float FallResistance = 0f;

        #if UNITY_EDITOR
        public void ForceValidTargetRadius()
        {
            LoseTargetRadius = DetectionRadius + 1f;
        }
        #endif
    }
}