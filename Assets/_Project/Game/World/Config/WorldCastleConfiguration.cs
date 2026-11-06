using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.World.Data;

namespace Wordania.World.Config
{
    /// <summary>
    /// The great wall (a vertical "horizon line") on the right part of the world and the castle biomes behind it.
    /// Castle biomes are shaped like the outer ones (smaller, warped) and their underground is built from
    /// horizontal floors/ceilings with shafts instead of caves.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldCastleConfiguration", menuName = "World/Castle Configuration")]
    public sealed class WorldCastleConfiguration : ScriptableObject
    {
        [Header("Great Wall")]
        [Tooltip("Base X of the wall as a fraction of the world width.")]
        [Range(0f, 1f)] public float WallPosition = 0.67f;
        public float WallFrequency = 0.01f;
        [Range(1, 8)] public int WallOctaves = 4;
        public float WallPersistence = 0.5f;
        public float WallLacunarity = 2f;
        [Tooltip("Horizontal swing of the wall (like HeightMultiplier for the horizon, but centred).")]
        public float WallAmplitude = 60f;
        [Min(1)] public int WallMinThickness = 8;
        [Min(1)] public int WallMaxThickness = 14;
        public float WallThicknessFrequency = 0.03f;
        [Tooltip("How far the wall rises above the highest ground under it.")]
        [Min(0)] public int WallHeightAboveSurface = 40;
        [Min(1)] public int CrenelWidth = 3;
        [Min(0)] public int CrenelHeight = 3;
        [Tooltip("Empty = Underground block of the castle fallback biome.")]
        public BlockData WallBlock;
        [Tooltip("Outer caves fade out over this distance before the wall.")]
        [Min(0)] public int CaveFadeDistance = 15;

        [Header("Castle Biomes")]
        [Tooltip("Multiplies WorldSettings.BiomeNoiseFrequency -> castle biomes are smaller than the outer ones.")]
        [Min(0.01f)] public float BiomeFrequencyMultiplier = 1.75f;
        public float BiomeWarpFrequency = 0.02f;
        [Tooltip("Domain warp strength in tiles (ragged borders, also for the depth bands).")]
        [Min(0)] public float BiomeWarpAmplitude = 25f;
        [Tooltip("Empty = WorldBiomeConfiguration.DefaultFallbackBiome.")]
        public BiomePalette FallbackBiome;
        public CastleInteriorStyle FallbackInterior = new();
        public List<CastleBiomeRule> Biomes = new();

        [Header("Interior (global floor grid)")]
        [Tooltip("Vertical distance between floor lines. Biomes make taller halls by skipping slabs.")]
        [Min(4)] public int FloorPitch = 16;
        [Tooltip("Solid rock kept above the bottom of the world.")]
        [Min(1)] public int FloorBottomMargin = 6;
        public float FloorWaveFrequency = 0.02f;
        [Min(0)] public float FloorWaveAmplitude = 3f;
        [Tooltip("Frequency of the noise that decides which parts of a floor exist (SlabCoverage).")]
        public float SlabSegmentFrequency = 0.03f;
        public float ErosionScale = 0.08f;
        public float LedgeFrequency = 0.08f;

        [Header("Courtyard (open roof)")]
        [Tooltip("Ground layer kept under the courtyard above the first hall.")]
        [Min(1)] public int CourtyardCrustDepth = 5;
        [Tooltip("0 = no entrances from the courtyard into the halls.")]
        [Min(0)] public int CourtyardEntranceSpacing = 60;
        [Min(1)] public int CourtyardEntranceWidth = 4;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (WallMaxThickness < WallMinThickness) WallMaxThickness = WallMinThickness;
        }
#endif
    }

    [Serializable]
    public sealed class CastleBiomeRule
    {
        public string Name;
        public BiomePalette Palette;

        [Header("Spawn Conditions (Noise Thresholds)")]
        [Range(0f, 1f)] public float MinTemperature;
        [Range(0f, 1f)] public float MaxTemperature = 1f;
        [Range(0f, 1f)] public float MinDepth;
        [Range(0f, 1f)] public float MaxDepth = 1f;

        public CastleInteriorStyle Interior = new();
    }

    /// <summary>How a castle biome fills the global floor grid.</summary>
    [Serializable]
    public sealed class CastleInteriorStyle
    {
        [Header("Floors")]
        [Min(1)] public int SlabThickness = 3;
        [Tooltip("Part of the floor lines that exist. Low = slabs skipped = taller halls.")]
        [Range(0f, 1f)] public float SlabCoverage = 0.7f;
        [Tooltip("Ruin-like bites taken out of the slabs.")]
        [Range(0f, 1f)] public float Erosion = 0.1f;

        [Header("Shafts in floors")]
        [Tooltip("At most one shaft per this many columns. 0 = none.")]
        [Min(0)] public int HoleSpacing = 32;
        [Min(1)] public int HoleMinWidth = 3;
        [Min(1)] public int HoleMaxWidth = 7;
        [Range(0f, 1f)] public float HoleChance = 0.85f;

        [Header("Pillars")]
        [Tooltip("0 = no pillars.")]
        [Min(0)] public int PillarSpacing;
        [Min(1)] public int PillarWidth = 2;
        [Range(0f, 1f)] public float PillarChance = 0.6f;
        [Min(1)] public int PillarMinHallHeight = 10;
        [Tooltip("Chance that a pillar (wall, with PillarWidth >= 3) has a doorway at floor level. 0 = solid walls.")]
        [Range(0f, 1f)] public float DoorChance;
        [Min(2)] public int DoorHeight = 5;

        [Header("Ledges")]
        [Tooltip("Noise threshold (Perlin, mostly 0.3-0.7): 0 = none, 0.5 = about half of the hall length.")]
        [Range(0f, 1f)] public float LedgeChance = 0.4f;
        [Min(1)] public int LedgeMinHallHeight = 12;
    }
}
