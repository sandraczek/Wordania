using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.World.Config;

namespace Wordania.Enemies.Data
{
    [Serializable]
    public class EnemySpawnData
    {
        [SerializeField] public bool RequiresGround = true;
        [SerializeField] public float MaxDistanceToGround = 1.5f;
        [SerializeField] public Vector2 RequiredClearanceSize;
        [SerializeField] public float RequiredGroundSize = 2f;
        [Tooltip("Biomes this enemy spawns in. Empty = every biome.")]
        [SerializeField] public List<BiomePalette> Biomes = new();
        [Tooltip("Relative spawn weight among enemies allowed in the biome. 1 = normal, 0.1 = ten times rarer, 0 = never.")]
        [Min(0f)]
        [SerializeField] public float SpawnWeight = 1f;

        public bool CanSpawnIn(BiomePalette biome)
        {
            if (Biomes == null || Biomes.Count == 0) return true;
            return biome != null && Biomes.Contains(biome);
        }
    }
}