using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Attributes;
using Wordania.Core.Identifiers;

namespace Wordania.Features.World.Config
{
    [CreateAssetMenu(fileName = "WorldSettings", menuName = "World/WorldSettings")]
    public sealed class WorldSettings : ScriptableObject
    {
        public float TileSize = 1f;
        public int Width;
        public int Height;

        public int Seed;
        public int ChunkSize;
        [Layer] public int CollisionLayer;
        [Header("Rendering")]
        public int RenderingBatchSize = 10;

        [Header("Biomes")]
        public WorldBiomeConfiguration BiomeConfig;
        public float BiomeNoiseFrequency; // = 0.01f;

        [Header("Terrain")]
        public float HeightMultiplier; // = 50f;
        public int SurfaceOffset; // = half of height
        public float SurfaceFrequency; //  = 0.01f
        [Range(1, 8)]
        public int Octaves; // = 4
        public float Persistence; // = 0.5f
        public float Lacunarity; // = 2.0f

        public float StoneNoiseScale;
        public float StoneNoiseAmplitude;
        public float MinDirtLayerDepth;
        public float dirt_stoneTransitionMargin;

        public float dirt_wall_Terrain_Scale;
        public float dirt_wall_Terrain_Amplitude;

        [Header("Caves")]
        public float GlobalCaveDensity; // = 0.6f;
        public float MacroScale; // = 0.04f;
        public float MacroWeight; // = 0.6f;
        public float MicroScale; // = 0.14f;
        public float MicroWeight; // = 0.4f;
        public float CaveStartDepth; // = 0.85f;
        public float CaveFullDensityDepth; // = 0.65f;

        [Header("Features")]
        public WorldFeatureConfiguration FeatureConfig;
        [SerializeField] private List<BlockData> _replaceableBlocks = new();
        [HideInInspector] public readonly HashSet<AssetId> ReplaceableBlockIds = new();
        public float DirtStoneScale; // = 0.04f;
        public float DirtStoneThreshold; // = 0.05f;

        [Header("Other")]
        public string MainLayerName = "Main";
        public string BackgroundLayerName = "Background";
        public string DamageLayerName = "Damage";
        public string ForegroundLayerName = "Foreground";

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / TileSize),
                Mathf.FloorToInt(worldPos.y / TileSize)
            );
        }

        public Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(
                (x * TileSize) + (TileSize * 0.5f),
                (y * TileSize) + (TileSize * 0.5f),
                0
            );
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            ReplaceableBlockIds.Clear();
            foreach (BlockData block in _replaceableBlocks)
            {
                if (block == null) continue;
                ReplaceableBlockIds.Add(block.Id);
            }
        }
#endif
    }
}