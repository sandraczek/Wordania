using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Identifiers;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    public readonly struct CachedFeatureRule
    {
        public readonly AssetId BlockId;
        public readonly int MinSize;
        public readonly int MaxSize;
        public readonly float SpawnChance;
        public readonly float MinDepth;
        public readonly float MaxDepth;

        public CachedFeatureRule(AssetId id, int minSize, int maxSize, float chance, float minDepth, float maxDepth)
        {
            BlockId = id;
            MinSize = minSize;
            MaxSize = maxSize;
            SpawnChance = chance;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }
    }
    public sealed class WorldPassTerrain : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        public WorldPassTerrain(WorldSettings settings)
        {
            _settings = settings;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            BiomePalette currentPalette = _settings.BiomeConfig.DefaultFallbackBiome;

            AssetId airId = new(0);
            AssetId surfaceBlockId = currentPalette.SurfaceBlock.Id;
            AssetId subSurfaceBlockId = currentPalette.SubSurfaceBlock.Id;
            AssetId undergroundBlockId = currentPalette.UndergroundBlock.Id;

            int width = _settings.Width;
            int height = _settings.Height;
            int seed = _settings.Seed;
            float surfaceFrequency = _settings.SurfaceFrequency;
            int octaves = _settings.Octaves;
            float persistence = _settings.Persistence;
            float lacunarity = _settings.Lacunarity;
            float heightMultiplier = _settings.HeightMultiplier;
            int surfaceOffset = _settings.SurfaceOffset;
            float minDirtLayerDepth = _settings.MinDirtLayerDepth;
            float stoneNoiseScale = _settings.StoneNoiseScale;
            float stoneNoiseAmplitude = _settings.StoneNoiseAmplitude;
            float dirtWallTerrainScale = _settings.dirt_wall_Terrain_Scale;
            float dirtWallTerrainAmplitude = _settings.dirt_wall_Terrain_Amplitude;
            float dirtStoneTransitionMargin = _settings.dirt_stoneTransitionMargin;

            var stopwatch = Stopwatch.StartNew();

            for (int x = 0; x < width; x++)
            {
                int terrainHeight = CalculateFractalHeight(x, surfaceFrequency, octaves, persistence, lacunarity, heightMultiplier, surfaceOffset, seed, height);

                int stoneHeight = CalculateStoneHeight(x, terrainHeight, minDirtLayerDepth, stoneNoiseScale, stoneNoiseAmplitude, seed);

                float wallNoiseValue = Mathf.PerlinNoise((x + seed + 6767) * dirtWallTerrainScale, 0);
                int wallOffset = (int)((wallNoiseValue - 0.5f) * dirtWallTerrainAmplitude);
                int wallTerrainHeight = terrainHeight + wallOffset;


                for (int y = 0; y < height; y++)
                {
                    BiomePalette localPalette = data.BiomeMap[x + y * width];
                    if (localPalette != currentPalette)
                    {
                        currentPalette = localPalette;
                        surfaceBlockId = currentPalette.SurfaceBlock.Id;
                        subSurfaceBlockId = currentPalette.SubSurfaceBlock.Id;
                        undergroundBlockId = currentPalette.UndergroundBlock.Id;
                    }

                    if (y > terrainHeight)
                    {
                        data.GetTile(x, y).Main = airId;
                    }
                    else if (y >= terrainHeight - 1)
                    {
                        data.GetTile(x, y).Main = surfaceBlockId;
                    }
                    else if (y > stoneHeight + dirtStoneTransitionMargin)
                    {
                        data.GetTile(x, y).Main = subSurfaceBlockId;
                    }
                    else if (y < stoneHeight - dirtStoneTransitionMargin)
                    {
                        data.GetTile(x, y).Main = undergroundBlockId;
                    }
                    else
                    {
                        float stoneChance = Mathf.InverseLerp(stoneHeight + dirtStoneTransitionMargin, stoneHeight - dirtStoneTransitionMargin, y);
                        data.GetTile(x, y).Main = (Random.value < stoneChance) ? undergroundBlockId : subSurfaceBlockId;
                    }

                    if (y > wallTerrainHeight)
                    {
                        data.GetTile(x, y).Background = airId;
                    }
                    else if (y >= wallTerrainHeight - 1 || y > terrainHeight)
                    {
                        data.GetTile(x, y).Background = surfaceBlockId;
                    }
                    else if (y < stoneHeight - dirtStoneTransitionMargin)
                    {
                        data.GetTile(x, y).Background = undergroundBlockId;
                    }
                    else if (y > stoneHeight + dirtStoneTransitionMargin)
                    {
                        data.GetTile(x, y).Background = subSurfaceBlockId;
                    }
                    else
                    {
                        float stoneChance = Mathf.InverseLerp(stoneHeight + dirtStoneTransitionMargin, stoneHeight - dirtStoneTransitionMargin, y);
                        data.GetTile(x, y).Background = (Random.value < stoneChance) ? undergroundBlockId : subSurfaceBlockId;
                    }
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }

            int centerX = width / 2;
            int groundY = CalculateFractalHeight(centerX, surfaceFrequency, octaves, persistence, lacunarity, heightMultiplier, surfaceOffset, seed, height);
            data.SpawnPoint = new Vector2Int(centerX, groundY + 2);
        }

        private int CalculateFractalHeight(int x, float surfaceFrequency, int octaves, float persistence, float lacunarity, float heightMultiplier, int surfaceOffset, int seed, int height)
        {
            float normalizedHeight = GenerationNoise.Fbm1D(x + seed, surfaceFrequency, octaves, persistence, lacunarity);
            int finalHeight = Mathf.FloorToInt(normalizedHeight * heightMultiplier) + surfaceOffset;

            return Mathf.Clamp(finalHeight, 0, height - 1);
        }

        private int CalculateStoneHeight(int x, int terrainHeight, float minDirtLayerDepth, float stoneNoiseScale, float stoneNoiseAmplitude, int seed)
        {
            float baseStoneLevel = terrainHeight - minDirtLayerDepth;

            float variation = Mathf.PerlinNoise((x + seed + 123) * stoneNoiseScale, 0);

            float offset = (variation - 0.5f) * stoneNoiseAmplitude;

            return Mathf.RoundToInt(baseStoneLevel + offset);
        }
    }
}