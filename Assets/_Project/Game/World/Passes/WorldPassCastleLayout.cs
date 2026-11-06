using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    /// <summary>
    /// Computes the great wall line: the horizon algorithm turned 90 degrees (fractal noise over y gives x),
    /// with a noisy thickness. Runs first; the biome map, caves, castle interior and the wall itself use it.
    /// </summary>
    public sealed class WorldPassCastleLayout : IWorldGenerationPass
    {
        private const int WallSalt = 4242;
        private const int ThicknessSalt = 919;

        private readonly WorldSettings _settings;

        public WorldPassCastleLayout(WorldSettings settings)
        {
            _settings = settings;
        }

        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            WorldCastleConfiguration config = _settings.CastleConfig;
            if (config == null)
            {
                data.Castle = null;
                return;
            }

            int width = data.Width;
            int height = data.Height;
            int seed = _settings.Seed;
            float baseX = config.WallPosition * width;

            var left = new int[height];
            var right = new int[height];

            for (int y = 0; y < height; y++)
            {
                float swing = GenerationNoise.Fbm1D(y + seed + WallSalt, config.WallFrequency, config.WallOctaves,
                    config.WallPersistence, config.WallLacunarity);
                float center = baseX + (swing - 0.5f) * config.WallAmplitude;

                float thicknessNoise = Mathf.PerlinNoise((y + seed + ThicknessSalt) * config.WallThicknessFrequency, 0.37f);
                int thickness = Mathf.RoundToInt(Mathf.Lerp(config.WallMinThickness, config.WallMaxThickness, thicknessNoise));

                int l = Mathf.Clamp(Mathf.RoundToInt(center - thickness * 0.5f), 2, width - 3);
                left[y] = l;
                right[y] = Mathf.Clamp(l + thickness - 1, l, width - 3);
            }

            data.Castle = new CastleLayout(left, right);

            await UniTask.Yield();
            token.ThrowIfCancellationRequested();
        }
    }
}
