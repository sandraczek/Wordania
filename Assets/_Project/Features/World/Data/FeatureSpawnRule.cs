using UnityEngine;

namespace Wordania.Features.World.Config
{
    [System.Serializable]
    public struct FeatureSpawnRule
    {
        [Tooltip("The block to place (e.g., Copper Ore, Granite).")]
        public BlockData FeatureBlock;

        [Header("Vein Settings")]
        [Tooltip("Minimum blocks in a single cluster.")]
        public int MinClusterSize;

        [Tooltip("Maximum blocks in a single cluster.")]
        public int MaxClusterSize;

        [Tooltip("Chance to spawn a cluster per eligible block (e.g., 0.005 for 0.5% chance).")]
        [Range(0f, 1f)] public float SpawnChance;

        [Header("Depth Constraints (0.0 = Surface, 1.0 = Bottom)")]
        [Range(0f, 1f)] public float MinDepth;
        [Range(0f, 1f)] public float MaxDepth;
    }
}