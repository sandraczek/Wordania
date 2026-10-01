using UnityEngine;

namespace Wordania.Enemies.Config
{
    [CreateAssetMenu(fileName = "NewEnemySpawnSettings", menuName = "Enemies/Enemy Spawn Settings")]
    public class EnemySystemSettings : ScriptableObject
    {
        [Header("Spawn Radii (Annulus)")]
        public float InnerViewportRadius = 50f;
        
        public float OuterSpawnRadius = 100f;
        
        public float DespawnRadius = 150f;

        [Header("Timing & Limits")]
        [Min(0.1f)] public float SpawnIntervalAttempt = 1.5f;
        
        public int MaxActiveEnemies = 30;

        [Header("Culling")]
        public float CullingInterval = 1f;
    }
}