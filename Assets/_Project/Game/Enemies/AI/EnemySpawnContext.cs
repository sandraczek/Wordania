using Wordania.Gameplay;

namespace Wordania.Enemies.AI
{
    /// <summary>
    /// World knowledge handed to an enemy when it is spawned (copied into its blackboard).
    /// Random world spawns use <see cref="None"/>; encounters fill in what their enemies need to know.
    /// </summary>
    public readonly struct EnemySpawnContext
    {
        public static EnemySpawnContext None => default;

        /// <summary>Area the enemy guards / waits around (e.g. the burning pyre). May be null.</summary>
        public readonly WorldZone GuardZone;

        /// <summary>
        /// Replaces the template's default behaviour for this spawn (same enemy, different role,
        /// e.g. a bandit that guards a camp instead of wandering). Null = template default.
        /// </summary>
        public readonly EnemyBehaviourProfile Behaviour;

        public EnemySpawnContext(WorldZone guardZone, EnemyBehaviourProfile behaviour = null)
        {
            GuardZone = guardZone;
            Behaviour = behaviour;
        }
    }
}
