using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;

namespace Wordania.Features.World.Passes
{
    public class WorldPassBiomeMap : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;

        public WorldPassBiomeMap(WorldSettings settings)
        {
            _settings = settings;
        }

        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            int width = _settings.Width;
            int height = _settings.Height;
            int seed = _settings.Seed;
            float biomeNoiseFrequency = _settings.BiomeNoiseFrequency;

            var rules = _settings.BiomeConfig.Biomes;
            var fallback = _settings.BiomeConfig.DefaultFallbackBiome;

            var stopwatch = Stopwatch.StartNew();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float temperature = Mathf.PerlinNoise((x + seed - 105) * biomeNoiseFrequency, (y + seed - 478) * biomeNoiseFrequency);
                    float normalizedDepth = 1f - (y / height);

                    data.BiomeMap[x + y * width] = ResolveBiome(temperature, normalizedDepth, rules, fallback);
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }
        }

        private BiomePalette ResolveBiome(
            float temperature,
            float depth,
            IReadOnlyList<WorldBiomeConfiguration.BiomeSpawnRules> rules,
            BiomePalette fallback)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (temperature >= rule.MinTemperature && temperature <= rule.MaxTemperature &&
                    depth >= rule.MinDepth && depth <= rule.MaxDepth)
                {
                    return rule.Palette;
                }
            }

            return fallback;
        }
    }
}