#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Wordania.Structures.Authoring;
using Wordania.World.Data;
using Object = UnityEngine.Object;

namespace Wordania.Structures.Editor
{
    /// <summary>
    /// Converts between an authoring prefab (<see cref="StructureAuthoring"/>) and the baked data of a
    /// <see cref="StructureTemplate"/>. Bakes automatically every time an authoring prefab is saved in Prefab Mode.
    /// </summary>
    [InitializeOnLoad]
    public static class StructureBaker
    {
        static StructureBaker() => PrefabStage.prefabSaved += OnPrefabSaved;

        private static void OnPrefabSaved(GameObject root)
        {
            if (root.TryGetComponent(out StructureAuthoring authoring)) Bake(authoring, logSuccess: false);
        }

        // ---------------------------------------------------------------- prefab -> template

        public static bool Bake(StructureAuthoring authoring, bool logSuccess = true)
        {
            if (EditorUtility.IsPersistent(authoring))
            {
                // Prefab asset selected in the Project window: bake from a temporary copy of its contents.
                GameObject contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(authoring));
                try { return Bake(contents.GetComponent<StructureAuthoring>(), logSuccess); }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }

            StructureTemplate template = authoring.Template;
            if (template == null) return Fail(authoring, "assign the Template to bake into.");
            if (authoring.Main == null) return Fail(authoring, "assign the Main tilemap.");

            var errors = new List<string>();
            Dictionary<Vector2Int, TileBase> mainTiles = CollectTiles(authoring, authoring.Main);
            Dictionary<Vector2Int, TileBase> backgroundTiles = authoring.Background != null
                ? CollectTiles(authoring, authoring.Background)
                : new Dictionary<Vector2Int, TileBase>();
            StructureMarker[] markers = authoring.GetComponentsInChildren<StructureMarker>(true);
            StructureEnemySpawn[] enemies = authoring.GetComponentsInChildren<StructureEnemySpawn>(true);

            // Bounds of everything -> the structure's size; its bottom-left cell becomes (0,0).
            var min = new Vector2Int(int.MaxValue, int.MaxValue);
            var max = new Vector2Int(int.MinValue, int.MinValue);
            void Include(Vector2Int cell)
            {
                min = Vector2Int.Min(min, cell);
                max = Vector2Int.Max(max, cell);
            }

            foreach (Vector2Int cell in mainTiles.Keys) Include(cell);
            foreach (Vector2Int cell in backgroundTiles.Keys) Include(cell);
            foreach (StructureMarker marker in markers)
            {
                Vector2Int cell = authoring.ToCell(marker.transform.position);
                Include(cell);
                Include(cell + marker.Size - Vector2Int.one);
            }
            foreach (StructureEnemySpawn enemy in enemies) Include(authoring.ToCell(enemy.transform.position));

            if (min.x > max.x) return Fail(authoring, "the structure is empty - paint some tiles first.");

            int width = max.x - min.x + 1;
            int height = max.y - min.y + 1;
            int Index(Vector2Int cell) => cell.x - min.x + (cell.y - min.y) * width;

            // Tiles -> cell codes.
            Dictionary<TileBase, BlockData> blocks = BuildTileLookup();
            var palette = new List<BlockData>();
            var paletteIndex = new Dictionary<BlockData, int>();
            int BlockCode(BlockData block)
            {
                if (!paletteIndex.TryGetValue(block, out int index))
                {
                    index = palette.Count;
                    palette.Add(block);
                    paletteIndex[block] = index;
                }
                return StructureTemplate.FirstBlockCode + index;
            }

            var main = new int[width * height]; // 0 = KeepCode
            foreach (KeyValuePair<Vector2Int, TileBase> pair in mainTiles)
            {
                if (pair.Value is StructureAirTile) main[Index(pair.Key)] = StructureTemplate.AirCode;
                else if (blocks.TryGetValue(pair.Value, out BlockData block)) main[Index(pair.Key)] = BlockCode(block);
                else errors.Add($"Main {pair.Key}: tile '{pair.Value.name}' is not the Tile of any BlockData.");
            }

            var background = new int[width * height];
            foreach (KeyValuePair<Vector2Int, TileBase> pair in backgroundTiles)
            {
                if (pair.Value is StructureAirTile)
                    Debug.LogWarning($"[Structures] {authoring.name}: Background {pair.Key} has the air tile - the background can only be kept or replaced, ignored.", authoring);
                else if (blocks.TryGetValue(pair.Value, out BlockData block)) background[Index(pair.Key)] = BlockCode(block);
                else errors.Add($"Background {pair.Key}: tile '{pair.Value.name}' is not the Tile of any BlockData.");
            }

            // Markers.
            var bakedMarkers = new List<StructureTemplate.Marker>();
            var names = new HashSet<string>();
            foreach (StructureMarker marker in markers)
            {
                if (!names.Add(marker.name)) errors.Add($"two markers are named '{marker.name}' (names must be unique).");
                bakedMarkers.Add(new StructureTemplate.Marker
                {
                    Name = marker.name,
                    Position = authoring.ToCell(marker.transform.position) - min,
                    Size = marker.Size,
                });
            }

            // Enemies.
            var bakedEnemies = new List<StructureTemplate.EnemySlot>();
            foreach (StructureEnemySpawn enemy in enemies)
            {
                if (enemy.Enemy == null) errors.Add($"enemy spawn '{enemy.name}' has no Enemy.");
                if (enemy.GuardZone != null && Array.IndexOf(markers, enemy.GuardZone) < 0)
                    errors.Add($"enemy spawn '{enemy.name}': its Guard Zone is not a marker of this structure.");

                bakedEnemies.Add(new StructureTemplate.EnemySlot
                {
                    Enemy = enemy.Enemy,
                    Cell = authoring.ToCell(enemy.transform.position) - min,
                    GuardZoneMarker = enemy.GuardZone != null ? enemy.GuardZone.name : string.Empty,
                    Behaviour = enemy.Behaviour,
                });
            }

            if (authoring.PlayerSpawn != null && Array.IndexOf(markers, authoring.PlayerSpawn) < 0)
                errors.Add("Player Spawn is not a marker of this structure.");

            if (errors.Count > 0)
            {
                foreach (string error in errors) Fail(authoring, error);
                return false;
            }

            template.EditorSetBaked(width, height, palette, main, background, bakedMarkers, bakedEnemies,
                authoring.PlayerSpawn != null ? authoring.PlayerSpawn.name : string.Empty);
            EditorUtility.SetDirty(template);
            AssetDatabase.SaveAssetIfDirty(template);

            if (logSuccess)
                Debug.Log($"[Structures] Baked '{authoring.name}' -> '{template.name}': {width}x{height}, " +
                          $"{bakedMarkers.Count} markers, {bakedEnemies.Count} enemies.", template);
            return true;
        }

        /// <summary>Non-empty cells of a tilemap, in the authoring's (main tilemap) cell coordinates.</summary>
        private static Dictionary<Vector2Int, TileBase> CollectTiles(StructureAuthoring authoring, Tilemap tilemap)
        {
            var cells = new Dictionary<Vector2Int, TileBase>();
            foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(position);
                if (tile != null) cells[authoring.ToCell(tilemap.GetCellCenterWorld(position))] = tile;
            }
            return cells;
        }

        private static bool Fail(StructureAuthoring authoring, string message)
        {
            Debug.LogError($"[Structures] Bake '{authoring.name}': {message}", authoring);
            return false;
        }

        // ---------------------------------------------------------------- template -> prefab

        /// <summary>Authoring prefab that bakes into this template (looked up by name: keep the template's name in it).</summary>
        public static GameObject FindAuthoringPrefab(StructureTemplate template)
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:Prefab {template.name}"))
            {
                var authoring = AssetDatabase.LoadAssetAtPath<StructureAuthoring>(AssetDatabase.GUIDToAssetPath(guid));
                if (authoring != null && authoring.Template == template) return authoring.gameObject;
            }
            return null;
        }

        /// <summary>Builds an authoring prefab from the template's baked data (empty for a new template).</summary>
        public static GameObject CreateAuthoringPrefab(StructureTemplate template)
        {
            string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(template));
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{template.name}_Authoring.prefab");
            StructureAirTile air = FindAirTile();

            var root = new GameObject(template.name, typeof(Grid), typeof(StructureAuthoring));
            try
            {
                Tilemap background = CreateTilemap(root.transform, "Background", 0);
                Tilemap main = CreateTilemap(root.transform, "Main", 1);

                for (int y = 0; y < template.Height; y++)
                {
                    for (int x = 0; x < template.Width; x++)
                    {
                        var position = new Vector3Int(x, y, 0);
                        switch (template.GetCell(x, y).Kind)
                        {
                            case StructureTemplate.CellKind.Air:
                                main.SetTile(position, air);
                                break;
                            case StructureTemplate.CellKind.Block:
                                main.SetTile(position, template.GetMainBlock(x, y).Tile);
                                break;
                        }

                        BlockData wall = template.GetBackgroundBlock(x, y);
                        if (wall != null) background.SetTile(position, wall.Tile);
                    }
                }

                Transform markerRoot = CreateChild(root.transform, "Markers");
                var markers = new Dictionary<string, StructureMarker>();
                foreach (StructureTemplate.Marker baked in template.Markers)
                {
                    var marker = CreateChild(markerRoot, baked.Name, baked.Position).gameObject.AddComponent<StructureMarker>();
                    bool isPlayerSpawn = baked.Name == template.PlayerSpawnMarker;
                    marker.EditorInitialize(baked.Size, isPlayerSpawn ? PlayerSpawnColor : ZoneColor);
                    markers[baked.Name] = marker;
                }

                Transform enemyRoot = CreateChild(root.transform, "Enemies");
                foreach (StructureTemplate.EnemySlot slot in template.Enemies)
                {
                    string name = slot.Enemy != null ? slot.Enemy.name : "Enemy";
                    var spawn = CreateChild(enemyRoot, name, slot.Cell).gameObject.AddComponent<StructureEnemySpawn>();
                    markers.TryGetValue(slot.GuardZoneMarker ?? string.Empty, out StructureMarker zone);
                    spawn.EditorInitialize(slot.Enemy, zone, slot.Behaviour);
                }

                markers.TryGetValue(template.PlayerSpawnMarker ?? string.Empty, out StructureMarker playerSpawn);
                root.GetComponent<StructureAuthoring>().EditorInitialize(template, main, background, playerSpawn);

                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        public static readonly Color ZoneColor = new(0.2f, 1f, 0.3f, 1f);
        public static readonly Color PlayerSpawnColor = new(1f, 0.85f, 0.2f, 1f);

        public static StructureAirTile FindAirTile()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(StructureAirTile)}");
            if (guids.Length == 0)
            {
                Debug.LogWarning("[Structures] No StructureAirTile asset found (Create > World > Structure Air Tile).");
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<StructureAirTile>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>Child placed in the middle of a cell (Grid cell size 1), so rounding never picks a neighbour.</summary>
        public static Transform CreateChild(Transform parent, string name, Vector2Int cell = default)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
            return child;
        }

        private static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<TilemapRenderer>().sortingOrder = sortingOrder;
            return go.GetComponent<Tilemap>();
        }

        /// <summary>Every BlockData's own tile -> the block (that is how painted tiles are recognised).</summary>
        private static Dictionary<TileBase, BlockData> BuildTileLookup()
        {
            var lookup = new Dictionary<TileBase, BlockData>();
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(BlockData)}"))
            {
                var block = AssetDatabase.LoadAssetAtPath<BlockData>(AssetDatabase.GUIDToAssetPath(guid));
                if (block == null || block.Tile == null) continue;

                if (lookup.TryGetValue(block.Tile, out BlockData other))
                    Debug.LogWarning($"[Structures] Blocks '{other.name}' and '{block.name}' share the tile '{block.Tile.name}'; structures will use '{other.name}'.");
                else
                    lookup[block.Tile] = block;
            }
            return lookup;
        }
    }
}
#endif
