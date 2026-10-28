using UnityEngine;
using Wordania.Combat;
using Wordania.Encounters;
using Wordania.Identifiers;

namespace Wordania.Enemies.AI
{
    /// <summary>
    /// The enemy's memory. Plain data only: sensors write facts here, states and conditions read them.
    /// Reset on every spawn (enemies are pooled).
    /// </summary>
    public sealed class EnemyBlackboard
    {
        // --- spawn / world context ---
        public Vector2 HomePosition;
        public ZoneArea GuardZone;

        // --- perception (written by sensors every physics step) ---
        public Entity NearestPlayer;
        public float NearestPlayerDistance = float.PositiveInfinity;

        // --- decisions ---
        /// <summary>Player the enemy is hunting. Kept until it dies or gets beyond LoseTargetRadius.</summary>
        public Entity Target;

        // --- events ---
        public InstanceId LastAttackerId;

        public bool HasTarget => Target != null && IsAlive(Target);
        public Vector2 TargetPosition => Target != null ? (Vector2)Target.Transform.position : HomePosition;

        public void Reset(Vector2 home, in EnemySpawnContext context)
        {
            HomePosition = home;
            GuardZone = context.GuardZone;
            NearestPlayer = null;
            NearestPlayerDistance = float.PositiveInfinity;
            Target = null;
            LastAttackerId = InstanceId.Empty;
        }

        public static bool IsAlive(Entity entity)
        {
            if (entity == null || !entity.isActiveAndEnabled) return false;
            return !entity.TryGetFeature(out HealthComponent health) || !health.IsDead;
        }
    }
}
