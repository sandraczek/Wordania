using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Data;
using Wordania.Enemies.AI;
using Wordania.Enemies.Data;
using Wordania.Identifiers;
using Wordania.World.Config;
using Wordania.World.Data;

namespace Wordania.Structures
{
    /// <summary>
    /// A hand-made piece of the world: blocks, named markers (zones, points) and enemy posts.
    /// Drawn in a <c>StructureAuthoring</c> prefab (Tilemaps + marker objects) and BAKED into this asset - the
    /// baked fields are never edited by hand. At runtime it is pure data: stamped into <see cref="WorldData"/>
    /// during world generation, its enemies spawned by <see cref="StructureService"/>.
    /// Local coordinates: (0,0) = bottom-left cell, x to the right, y up.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStructure", menuName = "World/Structure")]
    public sealed class StructureTemplate : DataAsset
    {
        // Baked cell codes; codes >= FirstBlockCode index the palette (FirstBlockCode + index),
        // negative codes are biome slots (BiomeCode) resolved by the biome where the cell is stamped.
        public const int KeepCode = 0;
        public const int AirCode = 1;
        public const int FirstBlockCode = 2;

        public static int BiomeCode(BiomeBlock slot) => -1 - (int)slot;

        public enum CellKind : byte
        {
            /// <summary>Leave the generated terrain as it is.</summary>
            Keep,
            /// <summary>Clear the main layer.</summary>
            Air,
            /// <summary>Place a block in the main layer.</summary>
            Block,
        }

        public readonly struct Cell
        {
            public readonly CellKind Kind;
            public readonly AssetId Main;
            /// <summary>Empty = keep the existing background.</summary>
            public readonly AssetId Background;

            public Cell(CellKind kind, AssetId main, AssetId background)
            {
                Kind = kind;
                Main = main;
                Background = background;
            }
        }

        /// <summary>Named area of cells (a zone, or a single point when 1x1).</summary>
        [Serializable]
        public struct Marker
        {
            public string Name;
            public Vector2Int Position;
            public Vector2Int Size;

            public RectInt Area => new(Position, Size);
        }

        /// <summary>One enemy post. Its index in <see cref="Enemies"/> is what saves remember as defeated.</summary>
        [Serializable]
        public struct EnemySlot
        {
            public EnemyTemplate Enemy;
            /// <summary>Cell the enemy stands in (feet on its bottom edge).</summary>
            public Vector2Int Cell;
            /// <summary>Marker whose area becomes the enemy's GuardZone. Empty = none.</summary>
            public string GuardZoneMarker;
            /// <summary>Overrides the enemy's default behaviour. Null = template default.</summary>
            public EnemyBehaviourProfile Behaviour;
        }

        public enum FoundationMode : byte
        {
            /// <summary>No foundation: the structure may hang over gaps.</summary>
            None,
            /// <summary>A block of the local biome (see Foundation Slot).</summary>
            Biome,
            /// <summary>One fixed block in every biome (see Foundation Block).</summary>
            Fixed,
        }

        [Header("Ground")]
        [Tooltip("What fills the gap under solid cells of the bottom row down to the ground.")]
        [SerializeField] private FoundationMode _foundationMode = FoundationMode.Biome;
        [Tooltip("Mode Biome: which block of the local biome palette is used (e.g. SubSurface = dirt, sand in a desert).")]
        [SerializeField] private BiomeBlock _foundationSlot = BiomeBlock.SubSurface;
        [Tooltip("Mode Fixed: the block used in every biome.")]
        [SerializeField] private BlockData _foundation;
        [SerializeField, Min(0)] private int _maxFoundationDepth = 16;
        [Tooltip("Surface placement only: over this many columns on each side the terrain is raised or cut so it " +
                 "meets the structure's floor at its edge (highest block with painted air above it; bottom row if none). 0 = off.")]
        [SerializeField, Min(0)] private int _terrainBlendWidth = 8;

        [Header("Gameplay")]
        [Tooltip("Enemies spawn when a player gets this close (world units) to the structure's center.")]
        [SerializeField, Min(1f)] private float _activationRadius = 50f;

        // ---- Baked from the authoring prefab (see StructureBaker). Do not edit by hand. ----
        [SerializeField, HideInInspector] private int _width;
        [SerializeField, HideInInspector] private int _height;
        [SerializeField, HideInInspector] private List<BlockData> _palette = new();
        /// <summary>Per cell (x + y * width): KeepCode, AirCode, FirstBlockCode + palette index or a BiomeCode.</summary>
        [SerializeField, HideInInspector] private int[] _main = Array.Empty<int>();
        /// <summary>Per cell: KeepCode, FirstBlockCode + palette index or a BiomeCode.</summary>
        [SerializeField, HideInInspector] private int[] _background = Array.Empty<int>();
        [SerializeField, HideInInspector] private List<Marker> _markers = new();
        [SerializeField, HideInInspector] private List<EnemySlot> _enemies = new();
        [SerializeField, HideInInspector] private string _playerSpawnMarker;

        /// <summary>Block filling the gap under the structure in a biome; empty id = no foundation.</summary>
        public AssetId GetFoundation(BiomePalette biome)
        {
            BlockData block = _foundationMode switch
            {
                FoundationMode.Biome => biome != null ? biome.Get(_foundationSlot) : null,
                FoundationMode.Fixed => _foundation,
                _ => null,
            };
            return block != null ? block.Id : default;
        }

        public bool HasFoundation => _foundationMode != FoundationMode.None;
        public int MaxFoundationDepth => _maxFoundationDepth;
        public int TerrainBlendWidth => _terrainBlendWidth;
        public float ActivationRadius => _activationRadius;

        public int Width => _width;
        public int Height => _height;
        public IReadOnlyList<Marker> Markers => _markers;
        public IReadOnlyList<EnemySlot> Enemies => _enemies;
        public string PlayerSpawnMarker => _playerSpawnMarker;

        /// <param name="biome">Biome at the cell, resolves biome slots. Null = biome cells have an empty block (Kind is still right).</param>
        public Cell GetCell(int x, int y, BiomePalette biome = null)
        {
            int i = x + y * _width;
            BlockData background = Resolve(_background[i], biome);
            AssetId backgroundId = background != null ? background.Id : default;

            switch (_main[i])
            {
                case KeepCode: return new Cell(CellKind.Keep, default, backgroundId);
                case AirCode: return new Cell(CellKind.Air, default, backgroundId);
            }

            BlockData main = Resolve(_main[i], biome);
            if (_main[i] < 0) return new Cell(CellKind.Block, main != null ? main.Id : default, backgroundId); // biome slot

            return main != null
                ? new Cell(CellKind.Block, main.Id, backgroundId)
                : new Cell(CellKind.Keep, default, backgroundId); // block asset missing from the palette
        }

        /// <summary>Main layer block at a local cell; null for keep/air (and biome slots without <paramref name="biome"/>).</summary>
        public BlockData GetMainBlock(int x, int y, BiomePalette biome = null) => Resolve(_main[x + y * _width], biome);

        /// <summary>Background block at a local cell; null = keep the existing background.</summary>
        public BlockData GetBackgroundBlock(int x, int y, BiomePalette biome = null) => Resolve(_background[x + y * _width], biome);

        /// <summary>True when the structure sets this cell's background (a block or a biome slot).</summary>
        public bool HasBackground(int x, int y) => _background[x + y * _width] != KeepCode;

        public bool TryGetMainBiomeSlot(int x, int y, out BiomeBlock slot) => TryGetBiomeSlot(_main[x + y * _width], out slot);

        public bool TryGetBackgroundBiomeSlot(int x, int y, out BiomeBlock slot) => TryGetBiomeSlot(_background[x + y * _width], out slot);

        public bool TryGetMarkerBounds(string marker, out RectInt bounds)
        {
            bounds = default;
            if (string.IsNullOrEmpty(marker)) return false;

            foreach (Marker m in _markers)
            {
                if (m.Name != marker) continue;
                bounds = m.Area;
                return true;
            }
            return false;
        }

        /// <summary>Cell where a new player appears (bottom-left cell of the player spawn marker).</summary>
        public bool TryGetPlayerSpawn(out Vector2Int cell)
        {
            bool found = TryGetMarkerBounds(_playerSpawnMarker, out RectInt bounds);
            cell = bounds.position;
            return found;
        }

        private BlockData Resolve(int code, BiomePalette biome)
        {
            if (code < 0)
                return biome != null && TryGetBiomeSlot(code, out BiomeBlock slot) ? biome.Get(slot) : null;

            int index = code - FirstBlockCode;
            return index >= 0 && index < _palette.Count ? _palette[index] : null;
        }

        private static bool TryGetBiomeSlot(int code, out BiomeBlock slot)
        {
            slot = (BiomeBlock)(-1 - code);
            return code < 0;
        }

#if UNITY_EDITOR
        /// <summary>Editor only: called by the baker.</summary>
        public void EditorSetBaked(int width, int height, List<BlockData> palette, int[] main, int[] background,
            List<Marker> markers, List<EnemySlot> enemies, string playerSpawnMarker)
        {
            _width = width;
            _height = height;
            _palette = palette;
            _main = main;
            _background = background;
            _markers = markers;
            _enemies = enemies;
            _playerSpawnMarker = playerSpawnMarker;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            int cells = _width * _height;
            if (_main.Length != cells || _background.Length != cells)
                Debug.LogWarning($"[Structure] {name}: baked data is corrupted ({_width}x{_height}, main {_main.Length}, " +
                                 $"background {_background.Length}). Bake the authoring prefab again.", this);
        }
#endif
    }
}
