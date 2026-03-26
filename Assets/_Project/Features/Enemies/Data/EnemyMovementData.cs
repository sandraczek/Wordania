using System;
using UnityEngine;
using Wordania.Features.Inventory;
using Wordania.Features.Movement;

namespace Wordania.Features.Enemies.Data
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

        //Ground Check
        [SerializeField] public LayerMask GroundLayer;
        [SerializeField] public float GroundCheckSizeY = 0.1f;
        [SerializeField] public float GroundCheckDistance = 0.2f;
        [SerializeField] public float MaxStepHeight = 1.1f;
        [SerializeField] public float StepLookMargin = 0.05f;
        [SerializeField] public float StepPerformMargin = 0.05f;
        [SerializeField] public float SkinWidth = 0.02f;
        [field: SerializeField] public float FallDamageThreshold = 35f;
        [field: SerializeField] public float FallDamageMultiplier = 3f;

        [Header("Cliff Avoidance")]
        [SerializeField] public bool EnableCliffAvoidance = true;
        [SerializeField] public float CliffDetectionOffset = 0.2f; 
        [SerializeField] public float CliffDetectionDepth = 4f; // should be slightly more than the enemy's height.
    }
}