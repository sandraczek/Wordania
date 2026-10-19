using System;
using UnityEngine;
using Wordania.Combat;

namespace Wordania.Enemies.Data
{
    [Serializable]
    public sealed class EnemyCombatData
    {
        [field: Header("Detection")]
        [field: SerializeField, Min(1f)] public float DetectionRadius { get; private set; } = 10f; //not used
        [field: SerializeField, Min(1f)] public float LoseTargetRadius { get; private set; } = 15f; //not used

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
    }
}