using System;
using UnityEngine;
using Wordania.Combat;

namespace Wordania.Bosses.Data
{
    /// <summary>
    /// Represents statistical data for a specific part of the boss (e.g., Head, Left Hand).
    /// </summary>
    [Serializable]
    public class BossPartData
    {
        [Min(0.01f)] [SerializeField] public float MaxHealth = 0f;
        [Min(0f)] [SerializeField] public float ContactDamage = 0f;
        [SerializeField] public Vector2 Knockback = new(15f,7f);
        [SerializeField] public DamageType DamageType = DamageType.Physical;
        [SerializeField] public HealthChangeSource DamageSource = HealthChangeSource.Generic;
        
        [Range(0f,1f)] [SerializeField] public float GeneralResistance = 0f;
        [Range(0f,1f)] [SerializeField] public float PhysicalResistance = 0f;
        [Range(0f,1f)] [SerializeField] public float MagicalResistance = 0f;
        [Range(0f,1f)] [SerializeField] public float EnvironmentalResistance = 0f;
        [Range(0f,1f)] [SerializeField] public float FallResistance = 1f;
    }
}