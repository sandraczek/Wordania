using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Structures;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    /// <summary>
    /// Places structures: the starting structure on the surface in the middle of the world (moves the world spawn
    /// onto its player spawn marker), then random ones from the configuration rules (surface or underground).
    /// Deterministic for a seed.
    /// </summary>
    public sealed class WorldPassStructures : IWorldGenerationPass
    {
        private const int SeedSalt = 0x5A17;
        private const int AttemptsPerStructure = 40;

        private readonly WorldSettings _settings;

        public WorldPassStructures(WorldSettings settings)
        {
            _settings = settings;
        }

        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            data.Structures.Clear();

            WorldStructureConfiguration config = _settings.StructureConfig;
            if (config != null)
            {
                var occupied = new List<RectInt>();

                if (config.StartingStructure != null)
                    PlaceStartingStructure(data, config, occupied);

                var random = new System.Random(_settings.Seed ^ SeedSalt);
                foreach (WorldStructureConfiguration.SpawnRule rule in config.Rules)
                {
                    if (rule.Template == null) continue;
                    for (int i = 0; i < rule.Count; i++)
                        TryPlaceRandom(data, config, rule, random, occupied);
                }
            }

            await UniTask.Yield();
            token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Structure is placed exactly in the middle of the world.
        /// </summary>
        private static void PlaceStartingStructure(WorldData data, WorldStructureConfiguration config, List<RectInt> occupied)
        {
            StructureTemplate template = config.StartingStructure;
            int originX = data.Width / 2 - template.Width / 2;
            int surface = StructureStamper.FindPlacementHeight(data, originX, template.Width, out _);
            if (surface < 0)
            {
                Debug.LogWarning($"[Structures] No ground for the starting structure '{template.name}'.");
                return;
            }

            var origin = new Vector2Int(originX, surface);
            StructureStamper.Stamp(data, template, origin);
            StructureStamper.BlendTerrain(data, template, origin);
            occupied.Add(GetFootprint(template, origin, config.Spacing, template.TerrainBlendWidth));

            if (template.TryGetPlayerSpawn(out Vector2Int cell))
                data.SpawnPoint = origin + cell;
        }

        private static void TryPlaceRandom(WorldData data, WorldStructureConfiguration config,
            WorldStructureConfiguration.SpawnRule rule, System.Random random, List<RectInt> occupied)
        {
            StructureTemplate template = rule.Template;
            int maxX = data.Width - template.Width - 2;
            if (maxX <= 2) return;

            for (int attempt = 0; attempt < AttemptsPerStructure; attempt++)
            {
                int originX = random.Next(2, maxX);
                int centerX = originX + template.Width / 2;
                if (Mathf.Abs(centerX - data.SpawnPoint.x) < rule.MinDistanceFromSpawn) continue;

                if (!TryFindOriginY(data, rule, originX, random, out int originY)) continue;

                var origin = new Vector2Int(originX, originY);
                bool surface = rule.Placement == WorldStructureConfiguration.Placement.Surface;
                int blend = surface ? template.TerrainBlendWidth : 0;
                RectInt footprint = GetFootprint(template, origin, config.Spacing, blend);
                if (Overlaps(footprint, occupied)) continue;

                StructureStamper.Stamp(data, template, origin);
                if (surface) StructureStamper.BlendTerrain(data, template, origin);
                occupied.Add(footprint);
                return;
            }

            Debug.LogWarning($"[Structures] Could not find a place for '{template.name}' ({AttemptsPerStructure} attempts).");
        }

        private static bool TryFindOriginY(WorldData data, WorldStructureConfiguration.SpawnRule rule, int originX,
            System.Random random, out int originY)
        {
            StructureTemplate template = rule.Template;
            originY = -1;

            switch (rule.Placement)
            {
                case WorldStructureConfiguration.Placement.Surface:
                    {
                        int surface = StructureStamper.FindPlacementHeight(data, originX, template.Width, out int variation);
                        if (surface < 0 || variation > rule.MaxSurfaceVariation) return false;
                        originY = surface;
                        break;
                    }
                case WorldStructureConfiguration.Placement.Underground:
                    {
                        int surface = StructureStamper.FindSurface(data, originX + template.Width / 2);
                        if (surface < 0) return false;

                        int depth = random.Next(rule.MinDepth, Mathf.Max(rule.MinDepth, rule.MaxDepth) + 1);
                        int top = surface - depth;          // structure's top row
                        originY = top - template.Height + 1; // origin = bottom row
                        break;
                    }
            }

            // Whole structure must fit between the bottom and top barriers.
            return originY > 0 && originY + template.Height < data.Height - 1;
        }

        /// <summary>
        /// Area a structure claims: its bounds grown by <paramref name="spacing"/> on every side, plus the terrain
        /// blend zone on the left and right. Structures may sit above each other (e.g. a dungeon under a village) as
        /// long as these areas do not overlap.
        /// </summary>
        private static RectInt GetFootprint(StructureTemplate template, Vector2Int origin, int spacing, int blend) =>
            new(origin.x - spacing - blend, origin.y - spacing,
                template.Width + (spacing + blend) * 2, template.Height + spacing * 2);

        private static bool Overlaps(RectInt rect, List<RectInt> others)
        {
            foreach (RectInt other in others)
                if (rect.Overlaps(other)) return true;
            return false;
        }
    }
}
