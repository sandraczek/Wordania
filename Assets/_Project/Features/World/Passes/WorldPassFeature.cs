using UnityEngine;
using System.Collections.Generic;
using Wordania.Features.World.Config;
using Wordania.Features.World.Data;
using System.Threading;
using Cysharp.Threading.Tasks;
using Wordania.Core.Identifiers;
using System.Diagnostics;

namespace Wordania.Features.World.Passes
{
    public class WorldPassFeature : IWorldGenerationPass
    {
        private readonly WorldSettings _settings;

        public WorldPassFeature(WorldSettings settings)
        {
            _settings = settings;
        }

        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            WorldFeatureConfiguration featureConfig = _settings.FeatureConfig;
            var cachedRules = new List<CachedFeatureRule>(featureConfig.Ores.Count + featureConfig.StoneVariations.Count);
            int width = _settings.Width;
            int height = _settings.Height;
            int seed = _settings.Seed;
            var replaceables = _settings.ReplaceableBlockIds;

            CacheRules(featureConfig.Ores, cachedRules);
            CacheRules(featureConfig.StoneVariations, cachedRules);

            var rng = new System.Random(seed ^ 42); // ^ 42 to offset it

            var stopwatch = Stopwatch.StartNew();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!replaceables.Contains(data.Tiles[x + y * width].M)) continue;

                    float depth = 1f - ((float)y / height);

                    for (int i = 0; i < cachedRules.Count; i++)
                    {
                        var rule = cachedRules[i];

                        if (depth < rule.MinDepth || depth > rule.MaxDepth) continue;

                        if (rng.NextDouble() <= rule.SpawnChance)
                        {
                            int clusterSize = rng.Next(rule.MinSize, rule.MaxSize + 1);
                            GenerateCluster(x, y, clusterSize, data, rule.BlockId, replaceables, rng);

                            break;
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

        private void GenerateCluster(int startX, int startY, int size, WorldData data, AssetId blockId, HashSet<AssetId> replaceables, System.Random rng)
        {
            int width = _settings.Width;
            int height = _settings.Height;
            int seed = _settings.Seed;

            int x = startX;
            int y = startY;

            for (int i = 0; i < size; i++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) break;

                if (replaceables.Contains(data.Tiles[x + y * width].M))
                {
                    data.Tiles[x + y * width].M = blockId;
                }

                int direction = rng.Next(0, 4);
                switch (direction)
                {
                    case 0: x++; break; // Right
                    case 1: x--; break; // Left
                    case 2: y++; break; // Up
                    case 3: y--; break; // Down
                }
            }
        }

        private void CacheRules(List<FeatureSpawnRule> rules, List<CachedFeatureRule> cache)
        {
            foreach (var rule in rules)
            {
                if (rule.FeatureBlock == null) continue;
                cache.Add(new CachedFeatureRule(
                    rule.FeatureBlock.Id,
                    rule.MinClusterSize,
                    rule.MaxClusterSize,
                    rule.SpawnChance,
                    rule.MinDepth,
                    rule.MaxDepth
                ));
            }
        }
    }
}