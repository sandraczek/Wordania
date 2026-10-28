using Wordania.Encounters;

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
        public readonly ZoneArea GuardZone;

        public EnemySpawnContext(ZoneArea guardZone)
        {
            GuardZone = guardZone;
        }
    }
}
