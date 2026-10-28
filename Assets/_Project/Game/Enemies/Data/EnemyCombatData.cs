using System;
using UnityEngine;
using Wordania.Combat;

namespace Wordania.Enemies.Data
{
    [Serializable]
    public sealed class EnemyCombatData
    {
        [field: Header("Detection")]
        [field: SerializeField, Min(1f)] public float DetectionRadius { get; private set; } = 10f;
        [field: SerializeField, Min(1f)] public float LoseTargetRadius { get; private set; } = 15f;

        [field: Header("Attacking")]
        [SerializeField] public float ContactDamage = 26f;
        [SerializeField] public Vector2 Knockback = new(15f, 7f);
        [SerializeField] public DamageType DamageType = DamageType.Physical;
        [SerializeField] public HealthChangeSource DamageSource = HealthChangeSource.Generic;

        [SerializeField] public float HitStunDuration = 0.2f;
        [SerializeField] public float InvincibilityDuration = 0.2f;
        [Range(0, 1)][SerializeField] public float GeneralResistance = 0f;
        [Range(0, 1)][SerializeField] public float PhysicalResistance = 0f;
        [Range(0, 1)][SerializeField] public float MagicalResistance = 0f;
        [Range(0, 1)][SerializeField] public float EnvironmentalResistance = 0f;
        [Range(0, 1)][SerializeField] public float FallResistance = 0f;

        [Header("Melee Attack (used by MeleeAttack state)")]
        [SerializeField] public EnemyMeleeAttackData Melee = new();
    }

    [Serializable]
    public sealed class EnemyMeleeAttackData
    {
        [Min(0f)] public float Damage = 20f;
        public Vector2 Knockback = new(12f, 6f);
        public DamageType DamageType = DamageType.Physical;
        public HealthChangeSource DamageSource = HealthChangeSource.Generic;

        [Tooltip("Telegraph time before the hit lands.")]
        [Min(0f)] public float WindupDuration = 0.35f;
        [Tooltip("Time standing still after the hit.")]
        [Min(0f)] public float RecoveryDuration = 0.5f;

        [Tooltip("Hitbox center relative to the enemy, x mirrored by facing.")]
        public Vector2 HitboxOffset = new(0.9f, 0f);
        public Vector2 HitboxSize = new(1.4f, 1.6f);
    }
}