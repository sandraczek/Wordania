using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Core.Config;
using Wordania.Core.Identifiers;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;

namespace Wordania.Features.World
{
    public sealed class WorldPassTransition : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        public WorldPassTransition(WorldSettings settings)
        {
            _settings = settings;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            BiomePalette currentPalette = _settings.BiomeConfig.DefaultFallbackBiome;
            AssetId subSurfaceBlockId = currentPalette.SubSurfaceBlock.Id;
            AssetId subSurfaceWallId = currentPalette.SubSurfaceBlock.Id;
            AssetId undergroundBlockId = currentPalette.SubSurfaceBlock.Id;
            AssetId undergroundWallId = currentPalette.SubSurfaceBlock.Id;

            int width = _settings.Width;

            var stopwatch = Stopwatch.StartNew();

            for (int x = 0; x < _settings.Width; x++)
            {
                for (int y = 0; y < _settings.Height; y++)
                {
                    BiomePalette localPalette = data.BiomeMap[x + y * width];
                    if (localPalette != currentPalette)
                    {
                        currentPalette = localPalette;
                        subSurfaceBlockId = currentPalette.SubSurfaceBlock.Id;
                        subSurfaceWallId = currentPalette.SubSurfaceWall.Id;
                        undergroundBlockId = currentPalette.UndergroundBlock.Id;
                        undergroundWallId = currentPalette.UndergroundWall.Id;
                    }
                    float stoneNoise = GetNoise(x, y, _settings.Seed + 2, _settings.DirtStoneScale);
                    float stoneValue = Mathf.Abs(stoneNoise - 0.5f);

                    if (stoneValue > _settings.DirtStoneThreshold)
                    {
                        if (data.GetTile(x, y).M == subSurfaceBlockId)
                        {
                            data.GetTile(x, y).M = undergroundBlockId;
                            continue;
                        }
                    }
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }
        }

        private float GetNoise(int x, int y, int seed, float scale)
        {
            return Mathf.PerlinNoise((x + seed) * scale, (y + seed) * scale);
        }
    }
}