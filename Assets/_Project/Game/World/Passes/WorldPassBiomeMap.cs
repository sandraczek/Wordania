using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    public class WorldPassBiomeMap : IWorldGenerationPass
    {
        private const int WarpSaltX = 31;
        private const int WarpSaltY = 577;
        private const int CastleTemperatureSaltX = 913;
        private const int CastleTemperatureSaltY = -271;

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

            CastleLayout castle = data.Castle;
            WorldCastleConfiguration castleConfig = _settings.CastleConfig;
            BiomePalette castleFallback = castleConfig != null && castleConfig.FallbackBiome != null
                ? castleConfig.FallbackBiome
                : fallback;

            var stopwatch = Stopwatch.StartNew();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (castle != null && castle.IsCastleSide(x, y))
                    {
                        data.BiomeMap[x + y * width] = ResolveCastleBiome(x, y, height, seed, biomeNoiseFrequency, castleConfig, castleFallback);
                        continue;
                    }

                    float temperature = Mathf.PerlinNoise((x + seed - 105) * biomeNoiseFrequency, (y + seed - 478) * biomeNoiseFrequency);
                    float normalizedDepth = 1f - ((float)y / height);

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

        /// <summary>
        /// Same temperature x depth lookup as outside, but with a higher frequency (smaller biomes) and a domain warp
        /// on both axes (ragged borders, depth bands are not straight lines).
        /// </summary>
        private static BiomePalette ResolveCastleBiome(int x, int y, int height, int seed, float biomeNoiseFrequency,
            WorldCastleConfiguration config, BiomePalette fallback)
        {
            float warpX = (Mathf.PerlinNoise((x + seed + WarpSaltX) * config.BiomeWarpFrequency, (y + seed) * config.BiomeWarpFrequency) - 0.5f) * 2f;
            float warpY = (Mathf.PerlinNoise((x + seed) * config.BiomeWarpFrequency, (y + seed + WarpSaltY) * config.BiomeWarpFrequency) - 0.5f) * 2f;
            float wx = x + warpX * config.BiomeWarpAmplitude;
            float wy = y + warpY * config.BiomeWarpAmplitude;

            float frequency = biomeNoiseFrequency * config.BiomeFrequencyMultiplier;
            float temperature = Mathf.PerlinNoise((wx + seed + CastleTemperatureSaltX) * frequency, (wy + seed + CastleTemperatureSaltY) * frequency);
            float depth = Mathf.Clamp01(1f - wy / height);

            List<CastleBiomeRule> rules = config.Biomes;
            for (int i = 0; i < rules.Count; i++)
            {
                CastleBiomeRule rule = rules[i];
                if (rule.Palette == null) continue;
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