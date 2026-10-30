using System.Collections.Generic;
using UnityEngine;
using Wordania.World;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.Structures
{
    /// <summary>Writes structure templates into world tiles. Generation-time only, no Unity objects involved.</summary>
    public static class StructureStamper
    {
        /// <summary>Y of the highest solid main tile in the column (below the top barrier), or -1.</summary>
        public static int FindSurface(WorldData data, int x)
        {
            for (int y = data.Height - 2; y > 0; y--)
                if (data.GetTile(x, y).Main.Hash != 0) return y;
            return -1;
        }

        /// <summary>
        /// Height for a structure spanning [originX, originX + width): the median surface, so a single spike or
        /// pit does not lift/sink the whole structure. Variation = highest - lowest surface (flatness check).
        /// </summary>
        public static int FindPlacementHeight(WorldData data, int originX, int width, out int variation)
        {
            var surfaces = new List<int>(width);
            for (int x = originX; x < originX + width; x++)
            {
                int surface = FindSurface(data, x);
                if (surface >= 0) surfaces.Add(surface);
            }

            if (surfaces.Count == 0)
            {
                variation = int.MaxValue;
                return -1;
            }

            surfaces.Sort();
            variation = surfaces[surfaces.Count - 1] - surfaces[0];
            return surfaces[surfaces.Count / 2];
        }

        /// <summary>Stamps the template with its bottom-left cell at <paramref name="origin"/> and records it in the world.</summary>
        public static PlacedStructure Stamp(WorldData data, StructureTemplate template, Vector2Int origin)
        {
            for (int y = 0; y < template.Height; y++)
            {
                for (int x = 0; x < template.Width; x++)
                {
                    int wx = origin.x + x;
                    int wy = origin.y + y;
                    if (!IsInterior(data, wx, wy)) continue; // never overwrite the world barrier

                    StructureTemplate.Cell cell = template.GetCell(x, y);
                    ref TileData tile = ref data.GetTile(wx, wy);

                    switch (cell.Kind)
                    {
                        case StructureTemplate.CellKind.Air:
                            tile.Main = default;
                            break;
                        case StructureTemplate.CellKind.Block:
                            tile.Main = cell.Main;
                            break;
                    }

                    if (!cell.Background.IsEmpty) tile.Background = cell.Background;
                }
            }

            StampFoundation(data, template, origin);

            var placed = new PlacedStructure(template.Id, origin);
            data.Structures.Add(placed);
            return placed;
        }

        private static void StampFoundation(WorldData data, StructureTemplate template, Vector2Int origin)
        {
            if (template.Foundation == null) return;

            for (int x = 0; x < template.Width; x++)
            {
                if (template.GetCell(x, 0).Kind != StructureTemplate.CellKind.Block) continue;

                int wx = origin.x + x;
                for (int depth = 1; depth <= template.MaxFoundationDepth; depth++)
                {
                    int wy = origin.y - depth;
                    if (!IsInterior(data, wx, wy)) break;

                    ref TileData tile = ref data.GetTile(wx, wy);
                    if (tile.Main.Hash != 0) break; // reached the ground
                    tile.Main = template.Foundation.Id;
                }
            }
        }

        /// <summary>
        /// Reshapes the terrain beside a surface structure so the ground meets its floor instead of ending in a cliff
        /// or a wall: over <see cref="StructureTemplate.TerrainBlendWidth"/> columns on each side the ground height
        /// eases from the structure's edge floor to the column's own surface. Missing ground is filled with the biome's
        /// surface/subsurface blocks, excess ground is cut away.
        /// </summary>
        public static void BlendTerrain(WorldData data, StructureTemplate template, Vector2Int origin)
        {
            int width = template.TerrainBlendWidth;
            if (width <= 0) return;

            int leftFloor = origin.y + FindEdgeFloor(template, 0);
            int rightFloor = origin.y + FindEdgeFloor(template, template.Width - 1);

            AlignStructureBackground(data, template, origin, leftFloor, rightFloor);

            for (int d = 1; d <= width; d++)
            {
                float t = Mathf.SmoothStep(0f, 1f, d / (float)(width + 1));
                BlendColumn(data, origin.x - d, leftFloor, t);
                BlendColumn(data, origin.x + template.Width - 1 + d, rightFloor, t);
            }
        }

        /// <summary>
        /// In the structure's columns the terrain's background (where the template does not paint its own) ends at
        /// the floor line (eased from the left edge floor to the right one): cut above it, filled below it.
        /// The blend zone then rises from this line to the terrain's natural background.
        /// </summary>
        private static void AlignStructureBackground(WorldData data, StructureTemplate template, Vector2Int origin,
            int leftFloor, int rightFloor)
        {
            int lowest = origin.y - template.MaxFoundationDepth - 1;

            for (int lx = 0; lx < template.Width; lx++)
            {
                int x = origin.x + lx;
                if (x <= 0 || x >= data.Width - 1) continue;

                float k = template.Width > 1 ? lx / (float)(template.Width - 1) : 0f;
                int wallTop = Mathf.RoundToInt(Mathf.Lerp(leftFloor, rightFloor, k));

                for (int y = data.Height - 2; y > 0 && y >= lowest; y--)
                {
                    int ly = y - origin.y;
                    if (ly >= 0 && ly < template.Height && template.GetBackgroundBlock(lx, ly) != null) continue; // painted

                    ref TileData tile = ref data.GetTile(x, y);
                    if (y > wallTop)
                    {
                        tile.Background = default;
                    }
                    else if (tile.Background.IsEmpty)
                    {
                        BiomePalette palette = data.BiomeMap[x + y * data.Width];
                        bool surfaceWall = y >= wallTop - 1; // like WorldPassTerrain
                        tile.Background = surfaceWall ? palette.SurfaceWall.Id : palette.SubSurfaceWall.Id;
                    }
                    else if (y < origin.y)
                    {
                        break; // reached the terrain's own background under the structure
                    }
                }
            }
        }

        /// <summary>
        /// Local y of the floor in an edge column: the highest block with painted air directly above it.
        /// 0 when there is none (e.g. a solid wall) - the bottom row sits at the original surface height.
        /// </summary>
        private static int FindEdgeFloor(StructureTemplate template, int x)
        {
            for (int y = template.Height - 2; y >= 0; y--)
            {
                if (template.GetCell(x, y).Kind == StructureTemplate.CellKind.Block &&
                    template.GetCell(x, y + 1).Kind == StructureTemplate.CellKind.Air)
                    return y;
            }
            return 0;
        }

        private static void BlendColumn(WorldData data, int x, int floor, float t)
        {
            if (x <= 0 || x >= data.Width - 1) return;

            int surface = FindSurface(data, x);
            if (surface < 0) return;

            int target = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(floor, surface, t)), 1, data.Height - 2);

            // The terrain's background may reach well above its ground (wall hills). Next to the structure it ends at
            // the floor like inside it; the offset grows back to the natural one with the same curve as the ground.
            int wallTop = FindBackgroundTop(data, x);
            int naturalOffset = wallTop < 0 ? 0 : wallTop - surface;
            int newWallTop = Mathf.Clamp(target + Mathf.RoundToInt(naturalOffset * t), 0, data.Height - 2);

            if (target > surface) RaiseGround(data, x, surface, target);
            else if (target < surface) LowerGround(data, x, surface, target);

            AdjustBackground(data, x, target, newWallTop);
        }

        /// <summary>Y of the highest non-empty background tile in the column (below the top barrier), or -1.</summary>
        private static int FindBackgroundTop(WorldData data, int x)
        {
            for (int y = data.Height - 2; y > 0; y--)
                if (!data.GetTile(x, y).Background.IsEmpty) return y;
            return -1;
        }

        /// <summary>Removes background above <paramref name="newWallTop"/> and fills gaps up to it.</summary>
        private static void AdjustBackground(WorldData data, int x, int target, int newWallTop)
        {
            for (int y = data.Height - 2; y > newWallTop; y--)
                data.GetTile(x, y).Background = default;

            for (int y = target + 1; y <= newWallTop; y++)
            {
                ref TileData tile = ref data.GetTile(x, y);
                if (!tile.Background.IsEmpty) continue;
                tile.Background = data.BiomeMap[x + y * data.Width].SurfaceWall.Id;
            }
        }

        private static void RaiseGround(WorldData data, int x, int surface, int target)
        {
            // The old top layer is buried now: grass -> dirt.
            for (int y = Mathf.Max(1, surface - 1); y <= surface; y++)
            {
                if (y >= target - 1) continue;
                BiomePalette palette = data.BiomeMap[x + y * data.Width];
                ref TileData tile = ref data.GetTile(x, y);
                if (tile.Main == palette.SurfaceBlock.Id) tile.Main = palette.SubSurfaceBlock.Id;
            }

            for (int y = surface + 1; y <= target; y++)
            {
                BiomePalette palette = data.BiomeMap[x + y * data.Width];
                bool top = y >= target - 1; // two top layers, like WorldPassTerrain
                ref TileData tile = ref data.GetTile(x, y);
                tile.Main = top ? palette.SurfaceBlock.Id : palette.SubSurfaceBlock.Id;
                if (tile.Background.IsEmpty)
                    tile.Background = top ? palette.SurfaceWall.Id : palette.SubSurfaceWall.Id;
            }
        }

        private static void LowerGround(WorldData data, int x, int surface, int target)
        {
            for (int y = target + 1; y <= surface; y++)
                data.GetTile(x, y).Main = default;

            // The exposed layer becomes the new top: dirt -> grass.
            for (int y = Mathf.Max(1, target - 1); y <= target; y++)
            {
                BiomePalette palette = data.BiomeMap[x + y * data.Width];
                ref TileData tile = ref data.GetTile(x, y);
                if (tile.Main == palette.SubSurfaceBlock.Id) tile.Main = palette.SurfaceBlock.Id;
            }
        }

        private static bool IsInterior(WorldData data, int x, int y) =>
            x > 0 && y > 0 && x < data.Width - 1 && y < data.Height - 1;
    }
}
