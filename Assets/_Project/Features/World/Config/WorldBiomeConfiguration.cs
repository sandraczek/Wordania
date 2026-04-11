using UnityEngine;
using System.Collections.Generic;
using Wordania.Features.World.Data;

namespace Wordania.Features.World.Config
{
    [CreateAssetMenu(fileName = "WorldBiomeConfiguration", menuName = "World/Biome Configuration")]
    public sealed class WorldBiomeConfiguration : ScriptableObject
    {
        [System.Serializable]
        public struct BiomeSpawnRules
        {
            public string Name;
            public BiomePalette Palette;

            [Header("Spawn Conditions (Noise Thresholds)")]
            [Range(0f, 1f)] public float MinTemperature;
            [Range(0f, 1f)] public float MaxTemperature;

            [Range(0f, 1f)] public float MinDepth;
            [Range(0f, 1f)] public float MaxDepth;
        }

        [field: SerializeField] public BiomePalette DefaultFallbackBiome { get; private set; }

        [field: SerializeField] public List<BiomeSpawnRules> Biomes { get; private set; } = new();
    }
}