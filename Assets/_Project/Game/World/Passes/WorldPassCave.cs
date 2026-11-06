using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Identifiers;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
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

            CastleLayout castle = data.Castle;
            int castleFade = castle != null ? _settings.CastleConfig.CaveFadeDistance : 0;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (data.GetTile(x, y).Main.Hash == 0) continue;

                    // No caves behind the wall (castle interior pass), fading out before it.
                    int wallDistance = castle != null ? castle.DistanceOutside(x, y) : int.MaxValue;
                    if (wallDistance <= 0) continue;

                    float currentDepth = (float)y / height;
                    float depthMask = Mathf.InverseLerp(caveStartDepth, caveFullDensityDepth, currentDepth);

                    float macroNoise = GetNoise(x, y, seed, macroScale);
                    float microNoise = GetNoise(x, y, seed + 1, microScale);

                    float combinedNoise = (macroNoise * macroWeight) + (microNoise * microWeight);
                    combinedNoise *= depthMask;

                    if (wallDistance < castleFade) combinedNoise *= (float)wallDistance / castleFade;

                    if (combinedNoise > globalCaveDensity)
                    {
                        data.GetTile(x, y).Main = airId;
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