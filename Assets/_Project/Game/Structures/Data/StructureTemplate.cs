using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Data;
using Wordania.Enemies.AI;
using Wordania.Enemies.Data;
using Wordania.Identifiers;
using Wordania.World.Data;

namespace Wordania.Structures
{
    /// <summary>
    /// A hand-authored piece of the world drawn as ASCII art: blocks + named markers (player spawn, enemy posts,
    /// zone corners). Pure data - it is stamped into <see cref="WorldData"/> during world generation and its
    /// enemies are spawned at runtime by <see cref="StructureService"/>.
    /// Local coordinates: x from the left, y from the BOTTOM row (row list is written top to bottom).
    /// </summary>
    [CreateAssetMenu(fileName = "NewStructure", menuName = "World/Structure")]
    public sealed class StructureTemplate : DataAsset
    {
        public const char AirSymbol = '.';
        public const char KeepSymbol = '-';

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

        [Serializable]
        public struct LegendEntry
        {
            [Tooltip("Single character used in the rows.")]
            public string Symbol;
            [Tooltip("Main layer block. Empty = air.")]
            public BlockData Main;
            [Tooltip("Background wall. Empty = keep the existing background.")]
            public BlockData Background;
            [Tooltip("Optional marker name. Every cell with this symbol becomes a point of the marker.")]
            public string Marker;
        }

        [Serializable]
        public struct EnemySlot
        {
            public EnemyTemplate Enemy;
            [Tooltip("One enemy is spawned on every cell of this marker (standing on the cell below).")]
            public string AtMarker;
            [Tooltip("Marker whose bounding box becomes the enemy's GuardZone. Empty = none.")]
            public string GuardZoneMarker;
            [Tooltip("Overrides the enemy's default behaviour for this structure. Empty = template default.")]
            public EnemyBehaviourProfile Behaviour;
        }

        /// <summary>One enemy to spawn. Its index in <see cref="SpawnPoints"/> is what saves remember as defeated.</summary>
        public readonly struct SpawnPoint
        {
            public readonly int SlotIndex;
            public readonly Vector2Int Cell;

            public SpawnPoint(int slotIndex, Vector2Int cell)
            {
                SlotIndex = slotIndex;
                Cell = cell;
            }
        }

        [Header("Shape")]
        [Tooltip("Rows from TOP to bottom. '.' = air (clears terrain), '-' = keep terrain, other characters come from " +
                 "the legend. Surface structures: the bottom row replaces the surface row.")]
        [SerializeField] private List<string> _rows = new();
        [SerializeField] private List<LegendEntry> _legend = new();
        [Tooltip("Fills the gap under solid cells of the bottom row down to the ground. Empty = no foundation.")]
        [SerializeField] private BlockData _foundation;
        [SerializeField, Min(0)] private int _maxFoundationDepth = 16;

        [Header("Gameplay")]
        [Tooltip("Marker where a new player appears (used by the starting structure). Empty = none.")]
        [SerializeField] private string _playerSpawnMarker;
        [SerializeField] private List<EnemySlot> _enemies = new();
        [Tooltip("Enemies spawn when a player gets this close (world units) to the structure's center.")]
        [SerializeField, Min(1f)] private float _activationRadius = 50f;

        public BlockData Foundation => _foundation;
        public int MaxFoundationDepth => _maxFoundationDepth;
        public string PlayerSpawnMarker => _playerSpawnMarker;
        public IReadOnlyList<EnemySlot> Enemies => _enemies;
        public float ActivationRadius => _activationRadius;

        public int Width { get { EnsureParsed(); return _width; } }
        public int Height { get { EnsureParsed(); return _height; } }
        public IReadOnlyList<SpawnPoint> SpawnPoints { get { EnsureParsed(); return _spawnPoints; } }

        // Parsed cache (rebuilt lazily; invalidated on every inspector change).
        [NonSerialized] private bool _parsed;
        [NonSerialized] private int _width;
        [NonSerialized] private int _height;
        [NonSerialized] private Cell[] _cells;
        [NonSerialized] private Dictionary<string, List<Vector2Int>> _markers;
        [NonSerialized] private List<SpawnPoint> _spawnPoints;

        public Cell GetCell(int x, int y)
        {
            EnsureParsed();
            return _cells[x + y * _width];
        }

        public bool TryGetLegend(char symbol, out LegendEntry entry)
        {
            foreach (LegendEntry e in _legend)
            {
                if (!string.IsNullOrEmpty(e.Symbol) && e.Symbol[0] == symbol)
                {
                    entry = e;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        /// <summary>Raw symbol at a local cell (for editor previews).</summary>
        public char GetSymbol(int x, int y)
        {
            EnsureParsed();
            string row = _rows[_height - 1 - y] ?? string.Empty;
            return x < row.Length ? row[x] : KeepSymbol;
        }

        public bool TryGetMarkerCells(string marker, out IReadOnlyList<Vector2Int> cells)
        {
            EnsureParsed();
            cells = null;
            if (string.IsNullOrEmpty(marker) || !_markers.TryGetValue(marker, out List<Vector2Int> list)) return false;

            cells = list;
            return true;
        }

        /// <summary>Bounding box (in cells) of every cell of the marker, e.g. two corner cells define a zone.</summary>
        public bool TryGetMarkerBounds(string marker, out RectInt bounds)
        {
            bounds = default;
            if (!TryGetMarkerCells(marker, out IReadOnlyList<Vector2Int> cells)) return false;

            Vector2Int min = cells[0];
            Vector2Int max = cells[0];
            for (int i = 1; i < cells.Count; i++)
            {
                min = Vector2Int.Min(min, cells[i]);
                max = Vector2Int.Max(max, cells[i]);
            }

            bounds = new RectInt(min, max - min + Vector2Int.one);
            return true;
        }

        private void EnsureParsed()
        {
            if (_parsed) return;
            _parsed = true;

            _height = _rows.Count;
            _width = 0;
            foreach (string row in _rows)
                _width = Mathf.Max(_width, row?.Length ?? 0);

            _cells = new Cell[_width * _height];
            _markers = new Dictionary<string, List<Vector2Int>>();
            _spawnPoints = new List<SpawnPoint>();

            for (int r = 0; r < _height; r++)
            {
                string row = _rows[r] ?? string.Empty;
                int y = _height - 1 - r; // rows are written top to bottom

                for (int x = 0; x < _width; x++)
                {
                    char symbol = x < row.Length ? row[x] : KeepSymbol;
                    _cells[x + y * _width] = ParseCell(symbol, new Vector2Int(x, y));
                }
            }

            for (int slot = 0; slot < _enemies.Count; slot++)
            {
                if (!_markers.TryGetValue(_enemies[slot].AtMarker ?? string.Empty, out List<Vector2Int> cells)) continue;
                foreach (Vector2Int cell in cells)
                    _spawnPoints.Add(new SpawnPoint(slot, cell));
            }
        }

        private Cell ParseCell(char symbol, Vector2Int position)
        {
            if (symbol == AirSymbol) return new Cell(CellKind.Air, default, default);
            if (!TryGetLegend(symbol, out LegendEntry entry)) return new Cell(CellKind.Keep, default, default);

            if (!string.IsNullOrEmpty(entry.Marker))
            {
                if (!_markers.TryGetValue(entry.Marker, out List<Vector2Int> cells))
                    _markers[entry.Marker] = cells = new List<Vector2Int>();
                cells.Add(position);
            }

            AssetId background = entry.Background != null ? entry.Background.Id : default;
            return entry.Main != null
                ? new Cell(CellKind.Block, entry.Main.Id, background)
                : new Cell(CellKind.Air, default, background);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _parsed = false;

            for (int i = 0; i < _rows.Count; i++)
            {
                string row = _rows[i] ?? string.Empty;
                if (_rows.Count > 0 && row.Length != (_rows[0]?.Length ?? 0))
                    Debug.LogWarning($"[Structure] {name}: row {i} has {row.Length} chars, row 0 has {_rows[0]?.Length ?? 0}. Missing cells = keep terrain.", this);

                foreach (char c in row)
                {
                    if (c != AirSymbol && c != KeepSymbol && !TryGetLegend(c, out _))
                        Debug.LogWarning($"[Structure] {name}: symbol '{c}' (row {i}) is not in the legend. Treated as keep terrain.", this);
                }
            }

            for (int i = 0; i < _legend.Count; i++)
            {
                string symbol = _legend[i].Symbol;
                if (string.IsNullOrEmpty(symbol) || symbol.Length != 1 || symbol[0] == AirSymbol || symbol[0] == KeepSymbol)
                    Debug.LogWarning($"[Structure] {name}: legend entry {i} needs exactly one character other than '{AirSymbol}' and '{KeepSymbol}'.", this);
            }

            foreach (EnemySlot slot in _enemies)
            {
                if (!TryGetMarkerCells(slot.AtMarker, out _))
                    Debug.LogWarning($"[Structure] {name}: enemy marker '{slot.AtMarker}' does not exist in the rows.", this);
                if (!string.IsNullOrEmpty(slot.GuardZoneMarker) && !TryGetMarkerCells(slot.GuardZoneMarker, out _))
                    Debug.LogWarning($"[Structure] {name}: guard zone marker '{slot.GuardZoneMarker}' does not exist in the rows.", this);
            }

            if (!string.IsNullOrEmpty(_playerSpawnMarker) && !TryGetMarkerCells(_playerSpawnMarker, out _))
                Debug.LogWarning($"[Structure] {name}: player spawn marker '{_playerSpawnMarker}' does not exist in the rows.", this);
        }
#endif
    }
}
