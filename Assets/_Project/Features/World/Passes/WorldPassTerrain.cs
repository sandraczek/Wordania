using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Core.Config;

namespace Wordania.Gameplay.World
{
    public sealed class WorldPassTerrain : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        private readonly IBlockDatabase _database;  // for future id refactor
        public WorldPassTerrain(WorldSettings settings, IBlockDatabase database)
        {
            _settings = settings;
            _database = database;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            int airId = 0;
            int grassId = 1;
            int grassWallId = 1001;
            int dirtId = 2;
            int dirtWallId = 1002;
            int stoneId = 3;
            int stoneWallId = 1003;

            var stopwatch = Stopwatch.StartNew();
            
            for (int x = 0; x < _settings.Width; x++)
            {
                int terrainHeight = CalculateFractalHeight(x, _settings);

                int stoneHeight = CalculateStoneHeight(x, terrainHeight, _settings);

                float wallNoiseValue = Mathf.PerlinNoise((x + _settings.Seed + 6767) * _settings.dirt_wall_Terrain_Scale, 0);
                int wallOffset = (int)((wallNoiseValue - 0.5f) * _settings.dirt_wall_Terrain_Amplitude);
                int wallTerrainHeight = terrainHeight + wallOffset;


                for (int y = 0; y < _settings.Height; y++)
                {
                    if (y > terrainHeight) {
                        data.GetTile(x, y).M = airId;
                    } else if (y >= terrainHeight -1) {
                        data.GetTile(x, y).M = grassId;
                    } else if (y < stoneHeight - _settings.dirt_stoneTransitionMargin) {
                        data.GetTile(x, y).M = stoneId;
                    } else if (y > stoneHeight + _settings.dirt_stoneTransitionMargin) {
                        data.GetTile(x, y).M = dirtId;
                    } else {
                        float stoneChance = Mathf.InverseLerp(stoneHeight + _settings.dirt_stoneTransitionMargin, stoneHeight - _settings.dirt_stoneTransitionMargin, y);
                        data.GetTile(x, y).M = (Random.value < stoneChance) ? stoneId : dirtId;
                    }

                    if (y > wallTerrainHeight) {
                        data.GetTile(x, y).B = airId;
                    } else if (y >= wallTerrainHeight -1 || y > terrainHeight) {
                        data.GetTile(x,y).B = grassWallId;
                    } else if (y < stoneHeight - _settings.dirt_stoneTransitionMargin) {
                        data.GetTile(x,y).B = stoneWallId;
                    } else if (y > stoneHeight + _settings.dirt_stoneTransitionMargin) {
                        data.GetTile(x,y).B = dirtWallId;
                    } else {
                        float stoneChance = Mathf.InverseLerp(stoneHeight + _settings.dirt_stoneTransitionMargin, stoneHeight - _settings.dirt_stoneTransitionMargin, y);
                        data.GetTile(x, y).B = (Random.value < stoneChance) ? stoneWallId : dirtWallId;
                    }
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();
                    
                    stopwatch.Restart();
                }
            }

            int centerX = _settings.Width / 2;
            int groundY = CalculateFractalHeight(centerX, _settings); 
            data.SpawnPoint = new Vector2Int(centerX, groundY + 2);
        }

        private int CalculateFractalHeight(int x, WorldSettings settings)
        {
            float total = 0;
            float frequency = settings.SurfaceFrequency;
            float amplitude = 1f;
            float maxValue = 0;

            for (int i = 0; i < settings.Octaves; i++)
            {
                float sampleX = (x + settings.Seed) * frequency + (i * 1000);
                float noiseValue = Mathf.PerlinNoise(sampleX, 0);

                total += noiseValue * amplitude;
                maxValue += amplitude;

                amplitude *= settings.Persistence;
                frequency *= settings.Lacunarity;
            }

            float normalizedHeight = total / maxValue;
            int finalHeight = Mathf.FloorToInt(normalizedHeight * settings.HeightMultiplier) + settings.SurfaceOffset;

            return Mathf.Clamp(finalHeight, 0, settings.Height - 1);
        }

        private int CalculateStoneHeight(int x, int terrainHeight, WorldSettings settings)
        {
            float baseStoneLevel = terrainHeight - settings.MinDirtLayerDepth;

            float variation = Mathf.PerlinNoise((x + settings.Seed + 123) * settings.StoneNoiseScale, 0);
            
            float offset = (variation - 0.5f) * settings.StoneNoiseAmplitude;

            return Mathf.RoundToInt(baseStoneLevel + offset);
        }
    }
}