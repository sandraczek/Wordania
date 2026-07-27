using Unity.Mathematics;
using Wordania.Core.Identifiers;

namespace Wordania.Features.Combat.Data
{
    public struct ProjectileRuntimeData
    {
        // --- Lifecycle & Linking ---
        public bool IsActive;
        public int ViewInstanceId;
        public int DataId;
        public InstanceId InstigatorId;
        public int TargetFactionMask;

        // --- Physics & Transform ---
        public float2 CurrentPosition; // float2 instead of Vector2 for Burst Compiler optimizations
        public float2 PreviousPosition;
        public float2 Velocity;

        // --- Dynamic Stats ---
        public float RemainingLifetime;
        public float CurrentSpeed;
        public float GravityMultiplier;

        // --- Combat Modifiers ---
        public int RemainingPierces;
        public float DamageMultiplier;

    }
}