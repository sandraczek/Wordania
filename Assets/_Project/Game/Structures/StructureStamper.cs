using System.Collections.Generic;
using UnityEngine;
using Wordania.World;
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

        private static bool IsInterior(WorldData data, int x, int y) =>
            x > 0 && y > 0 && x < data.Width - 1 && y < data.Height - 1;
    }
}
