using UnityEngine;
using UnityEngine.Tilemaps;

namespace Wordania.Structures.Authoring
{
    /// <summary>
    /// Authoring-only tile: a structure cell painted with it clears the generated terrain (air).
    /// Blocks are painted with their own <c>BlockData.Tile</c>; an empty cell keeps the terrain.
    /// </summary>
    [CreateAssetMenu(fileName = "StructureAir", menuName = "World/Structure Air Tile")]
    public sealed class StructureAirTile : TileBase
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _color = new(0.45f, 0.75f, 1f, 0.35f);

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData)
        {
            tileData.sprite = _sprite;
            tileData.color = _color;
            tileData.transform = Matrix4x4.identity;
            tileData.flags = TileFlags.LockColor | TileFlags.LockTransform;
            tileData.colliderType = Tile.ColliderType.None;
        }
    }
}
