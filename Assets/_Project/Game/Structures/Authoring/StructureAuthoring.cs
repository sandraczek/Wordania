using UnityEngine;
using UnityEngine.Tilemaps;

namespace Wordania.Structures.Authoring
{
    /// <summary>
    /// Editor-side source of a <see cref="StructureTemplate"/> (root of an authoring prefab, never used at runtime).
    /// <list type="bullet">
    /// <item>Main / Background tilemaps: paint with the blocks' own tiles (<c>BlockData.Tile</c>).
    /// <see cref="StructureAirTile"/> carves air, an empty cell keeps the generated terrain.</item>
    /// <item><see cref="StructureMarker"/> children: named areas (guard zones, player spawn, ...).</item>
    /// <item><see cref="StructureEnemySpawn"/> children: enemy posts.</item>
    /// </list>
    /// The bounding box of everything becomes the structure; its bottom-left cell is the origin.
    /// Baked into the template when the prefab is saved (or with the inspector button).
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public sealed class StructureAuthoring : MonoBehaviour
    {
        [SerializeField] private StructureTemplate _template;
        [SerializeField] private Tilemap _main;
        [SerializeField] private Tilemap _background;
        [Tooltip("Where a new player appears (only used by the starting structure). Empty = none.")]
        [SerializeField] private StructureMarker _playerSpawn;

        public StructureTemplate Template => _template;
        public Tilemap Main => _main;
        public Tilemap Background => _background;
        public StructureMarker PlayerSpawn => _playerSpawn;

        /// <summary>Cell (in main tilemap coordinates) containing a world position.</summary>
        public Vector2Int ToCell(Vector3 world)
        {
            Vector3Int cell = _main != null ? _main.WorldToCell(world) : GetComponent<Grid>().WorldToCell(world);
            return new Vector2Int(cell.x, cell.y);
        }

        /// <summary>World position of a cell's bottom-left corner.</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            var c = new Vector3Int(cell.x, cell.y, 0);
            return _main != null ? _main.CellToWorld(c) : GetComponent<Grid>().CellToWorld(c);
        }

#if UNITY_EDITOR
        public void EditorInitialize(StructureTemplate template, Tilemap main, Tilemap background, StructureMarker playerSpawn)
        {
            _template = template;
            _main = main;
            _background = background;
            _playerSpawn = playerSpawn;
        }
#endif
    }
}
