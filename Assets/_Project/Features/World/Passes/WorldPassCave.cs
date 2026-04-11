using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Core.Config;
using Wordania.Core.Identifiers;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;

namespace Wordania.Features.World
{
    public sealed class WorldPassCave : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;
        public WorldPassCave(WorldSettings settings)
        {
            _settings = settings;
        }
        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            AssetId airId = new(0);

            int width = _settings.Width;
            int height = _settings.Height;
            int seed = _settings.Seed;
            float microScale = _settings.MicroScale;
            float macroScale = _settings.MacroScale;
            float microWeight = _settings.MacroWeight;
            float macroWeight = _settings.MacroWeight;
            float caveStartDepth = _settings.CaveStartDepth;
            float caveFullDensityDepth = _settings.CaveFullDensityDepth;
            float globalCaveDensity = _settings.GlobalCaveDensity;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (data.GetTile(x, y).M.Hash == 0) continue;

                    float currentDepth = (float)y / height;
                    float depthMask = Mathf.InverseLerp(caveStartDepth, caveFullDensityDepth, currentDepth);

                    float macroNoise = GetNoise(x, y, seed, macroScale);
                    float microNoise = GetNoise(x, y, seed + 1, microScale);

                    float combinedNoise = (macroNoise * macroWeight) + (microNoise * microWeight);
                    combinedNoise *= depthMask;

                    if (combinedNoise > globalCaveDensity)
                    {
                        data.GetTile(x, y).M = airId;
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