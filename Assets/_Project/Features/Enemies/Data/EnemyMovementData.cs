using System;
using UnityEngine;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.Movement;

namespace Wordania.Gameplay.Enemies.Data
{
    [Serializable]
    public sealed class EnemyMovementData
    {
        [SerializeField] public float GravityScale = 5f;
        [SerializeField] public float Acceleration = 50f;
        [SerializeField] public float Deceleration = 30f;
        [SerializeField] public float PatrolSpeed { get; private set; } = 2f;
        [SerializeField] public float PatrolIntervalTime = 5f;
        [SerializeField] public float ChaseSpeed { get; private set; } = 4.5f;
        [SerializeField] public float JumpForce { get; private set; } = 10f;
        [SerializeField] public LayerMask GroundLayer;
        [SerializeField] public float GroundCheckSizeY = 0.1f;
        [SerializeField] public float GroundCheckDistance = 0.2f;
        [SerializeField] public float MaxStepHeight = 1.1f;
        [SerializeField] public float StepLookDistance = 0.2f;
        [field: SerializeField] public bool AvoidsLedges { get; private set; } = true;
        [field: SerializeField] public float FallDamageThreshold = 35f;
        [field: SerializeField] public float FallDamageMultiplier = 3f;
    }
}