using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wordania.Identifiers;
using Wordania.Structures;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.World.Passes
{
    /// <summary>
    /// Replaces caves behind the great wall with open halls. A global grid of wavy floor lines (FloorPitch apart)
    /// is shared by all castle biomes so floors line up across biome borders; each biome only decides how it fills
    /// the grid (<see cref="CastleInteriorStyle"/>): slab thickness and coverage (skipped slabs = taller halls),
    /// shafts through slabs, erosion, pillars and ledges. The courtyard (surface) stays open to the sky with a thin
    /// ground crust and a few entrance shafts.
    /// Only carves air: everything that is not hall keeps the terrain blocks of its biome.
    /// </summary>
    public sealed class WorldPassCastleInterior : IWorldGenerationPass
    {
        private const int HoleSalt = 0x3C71;
        private const int HoleChanceSalt = 0x51A3;
        private const int PillarSalt = 0x7E21;
        private const int DoorSalt = 0x4F13;
        private const int EntranceSalt = 0x2B9F;
        private const int EntranceLine = -1;

        private readonly WorldSettings _settings;

        public WorldPassCastleInterior(WorldSettings settings)
        {
            _settings = settings;
        }

        public async UniTask Execute(CancellationToken token, WorldData data)
        {
            CastleLayout castle = data.Castle;
            WorldCastleConfiguration config = _settings.CastleConfig;
            if (castle == null || config == null) return;

            var styles = new Dictionary<BiomePalette, CastleInteriorStyle>();
            foreach (CastleBiomeRule rule in config.Biomes)
            {
                if (rule.Palette != null && rule.Interior != null && !styles.ContainsKey(rule.Palette))
                    styles.Add(rule.Palette, rule.Interior);
            }
            CastleInteriorStyle fallbackStyle = config.FallbackInterior ?? new CastleInteriorStyle();

            AssetId airId = new(0);
            int width = data.Width;
            int height = data.Height;
            int seed = _settings.Seed;
            int pitch = config.FloorPitch;
            int bottom = config.FloorBottomMargin;
            int lineCount = (height - bottom) / pitch + 2;

            var lines = new int[lineCount];
            var solid = new bool[height];

            var stopwatch = Stopwatch.StartNew();

            for (int x = Mathf.Max(castle.MinLeft, 1); x < width - 1; x++)
            {
                int surface = StructureStamper.FindSurface(data, x);
                if (surface < 0) continue;
                // Rows [crustBottom, surface] stay as courtyard ground; halls end below it.
                int crustBottom = surface - config.CourtyardCrustDepth + 1;

                for (int j = 0; j < lineCount; j++)
                    lines[j] = bottom + j * pitch + Wave(x, j, seed, config);

                Array.Clear(solid, 0, height);

                for (int j = 0; j < lineCount; j++)
                {
                    int lineY = lines[j];
                    if (lineY >= crustBottom) break;

                    CastleInteriorStyle slabStyle = StyleAt(data, x, lineY, styles, fallbackStyle);
                    int slabEnd = Mathf.Min(lineY + slabStyle.SlabThickness, crustBottom);
                    // The topmost floor would leave a hall too low to walk in -> leave it out (hall below merges up).
                    if (crustBottom - slabEnd < config.CourtyardMinHallHeight) break;
                    if (IsSlabPresent(x, j, seed, config, slabStyle))
                    {
                        for (int y = Mathf.Max(lineY, 0); y < slabEnd; y++)
                            if (!IsEroded(x, y, seed, config, slabStyle)) solid[y] = true;
                    }

                    int hallBottom = Mathf.Max(slabEnd, 0);
                    int hallTop = crustBottom;
                    if (j + 1 < lineCount && lines[j + 1] < crustBottom)
                    {
                        // The next floor is skipped when its own hall would be too low (see the check above).
                        CastleInteriorStyle nextStyle = StyleAt(data, x, lines[j + 1], styles, fallbackStyle);
                        int nextSlabEnd = Mathf.Min(lines[j + 1] + nextStyle.SlabThickness, crustBottom);
                        if (crustBottom - nextSlabEnd >= config.CourtyardMinHallHeight) hallTop = lines[j + 1];
                    }
                    int hallHeight = hallTop - hallBottom;
                    if (hallHeight <= 0) continue;

                    CastleInteriorStyle hallStyle = StyleAt(data, x, hallBottom + hallHeight / 2, styles, fallbackStyle);
                    if (TryGetPillar(x, j, hallHeight, seed, hallStyle, out bool hasDoor))
                    {
                        // A wall with a doorway keeps its floor-level opening free.
                        int wallBottom = hasDoor ? hallBottom + Mathf.Min(hallStyle.DoorHeight, hallHeight - 1) : hallBottom;
                        for (int y = wallBottom; y < hallTop; y++) solid[y] = true;
                    }
                    else
                    {
                        AddLedges(solid, x, j, hallBottom, hallTop, seed, config, hallStyle);
                    }
                }

                bool entrance = IsInHole(x, EntranceLine, seed ^ EntranceSalt, config.CourtyardEntranceSpacing,
                    config.CourtyardEntranceWidth, config.CourtyardEntranceWidth, 1f);
                int carveTop = Mathf.Min(entrance ? surface : crustBottom - 1, height - 2);

                for (int y = bottom; y <= carveTop; y++)
                {
                    if (!castle.IsInside(x, y)) continue;
                    if (y < crustBottom && solid[y]) continue;
                    data.GetTile(x, y).Main = airId;
                }

                if (stopwatch.ElapsedMilliseconds > 16)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();

                    stopwatch.Restart();
                }
            }
        }

        private static CastleInteriorStyle StyleAt(WorldData data, int x, int y,
            Dictionary<BiomePalette, CastleInteriorStyle> styles, CastleInteriorStyle fallback)
        {
            y = Mathf.Clamp(y, 0, data.Height - 1);
            BiomePalette palette = data.BiomeMap[x + y * data.Width];
            return palette != null && styles.TryGetValue(palette, out CastleInteriorStyle style) ? style : fallback;
        }

        private static int Wave(int x, int line, int seed, WorldCastleConfiguration config)
        {
            float noise = Mathf.PerlinNoise((x + seed) * config.FloorWaveFrequency, line * 7.31f + 0.5f);
            return Mathf.RoundToInt((noise - 0.5f) * 2f * config.FloorWaveAmplitude);
        }

        private static bool IsSlabPresent(int x, int line, int seed, WorldCastleConfiguration config, CastleInteriorStyle style)
        {
            if (style.SlabCoverage < 1f)
            {
                float coverage = Mathf.PerlinNoise((x + seed) * config.SlabSegmentFrequency, line * 3.71f + 11.3f);
                if (coverage > style.SlabCoverage) return false;
            }

            return !IsInHole(x, line, seed ^ HoleSalt, style.HoleSpacing, style.HoleMinWidth, style.HoleMaxWidth, style.HoleChance);
        }

        private static bool IsEroded(int x, int y, int seed, WorldCastleConfiguration config, CastleInteriorStyle style)
        {
            if (style.Erosion <= 0f) return false;
            float noise = Mathf.PerlinNoise((x + seed) * config.ErosionScale + 0.13f, (y + seed) * config.ErosionScale + 0.71f);
            return noise < style.Erosion;
        }

        /// <summary>
        /// Splits the line into cells of <paramref name="spacing"/> columns (shifted per line, so shafts of
        /// neighbouring floors do not stack) and cuts at most one hole of random width at a random spot per cell.
        /// Guarantees a way up/down at least every 2 * spacing columns when chance = 1.
        /// </summary>
        private static bool IsInHole(int x, int line, int salt, int spacing, int minWidth, int maxWidth, float chance)
        {
            if (spacing <= 0 || chance <= 0f) return false;

            int shifted = x + (int)(GenerationNoise.Hash(line, 0, salt) % (uint)spacing);
            int cell = shifted / spacing;
            if (GenerationNoise.Hash01(cell, line, salt ^ HoleChanceSalt) >= chance) return false;

            uint hash = GenerationNoise.Hash(cell, line, salt);
            int max = Mathf.Max(minWidth, maxWidth);
            int holeWidth = Mathf.Min(minWidth + (int)(hash % (uint)(max - minWidth + 1)), spacing);
            int start = (int)((hash >> 12) % (uint)(spacing - holeWidth + 1));
            int local = shifted - cell * spacing;
            return local >= start && local < start + holeWidth;
        }

        private static bool TryGetPillar(int x, int line, int hallHeight, int seed, CastleInteriorStyle style, out bool hasDoor)
        {
            hasDoor = false;
            int spacing = style.PillarSpacing;
            if (spacing <= 0 || hallHeight < style.PillarMinHallHeight) return false;

            int salt = seed ^ PillarSalt;
            int shifted = x + (int)(GenerationNoise.Hash(line, 1, salt) % (uint)spacing);
            int index = shifted / spacing;
            int local = shifted - index * spacing;
            if (local >= style.PillarWidth || GenerationNoise.Hash01(index, line, salt) >= style.PillarChance) return false;

            hasDoor = style.DoorChance > 0f && GenerationNoise.Hash01(index, line, salt ^ DoorSalt) < style.DoorChance;
            return true;
        }

        /// <summary>
        /// Thin platforms in noise-driven runs, one tier every <see cref="CastleInteriorStyle.LedgeSpacing"/> rows,
        /// counted down from just below the hall ceiling (so the top tier is within jump range of the shafts above).
        /// Tiers use different noise offsets, so runs of neighbouring tiers rarely stack into a solid wall.
        /// </summary>
        private static void AddLedges(bool[] solid, int x, int line, int hallBottom, int hallTop, int seed,
            WorldCastleConfiguration config, CastleInteriorStyle style)
        {
            if (style.LedgeChance <= 0f || hallTop - hallBottom < style.LedgeMinHallHeight) return;

            int spacing = Mathf.Max(2, style.LedgeSpacing);
            int tier = 0;
            for (int y = hallTop - style.LedgeTopOffset; y >= hallBottom + 2; y -= spacing, tier++)
            {
                float noise = Mathf.PerlinNoise((x + seed) * config.LedgeFrequency, line * 5.17f + tier * 13.9f + 77.7f);
                if (noise < style.LedgeChance) solid[y] = true;
            }
        }
    }
}
