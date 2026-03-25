using UnityEngine;

namespace Wordania.Gameplay.Enemies.Config
{
    [CreateAssetMenu(fileName = "NewEnemySpawnSettings", menuName = "Enemies/Enemy Spawn Settings")]
    public class EnemySpawnSettings : ScriptableObject
    {
        [Header("Spawn Radii (Annulus)")]
        public float InnerViewportRadius = 50f;
        
        public float OuterSpawnRadius = 100f;
        
        public float DespawnRadius = 150f;

        [Header("Timing & Limits")]
        [Min(0.1f)] public float SpawnAttemptInterval = 1.5f;
        
        public int MaxActiveEnemies = 30;
    }
}