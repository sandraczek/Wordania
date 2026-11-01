using UnityEngine;
using UnityEngine.Tilemaps;
using Wordania.World.Config;

namespace Wordania.Structures.Authoring
{
    /// <summary>
    /// Authoring-only tile: a structure cell painted with it gets the <see cref="Slot"/> block of the biome it lands in
    /// (e.g. Surface = grass in plains, sand in the desert). Works on both the Main and Background tilemaps.
    /// In the editor it is drawn as the slot block of <see cref="_previewBiome"/>, tinted to tell it from fixed blocks.
    /// </summary>
    [CreateAssetMenu(fileName = "StructureBiome", menuName = "World/Structure Biome Tile")]
    public sealed class StructureBiomeTile : TileBase
    {
        [SerializeField] private BiomeBlock _slot;
        [Tooltip("Biome used only to draw the tile in the editor.")]
        [SerializeField] private BiomePalette _previewBiome;
        [SerializeField] private Color _tint = new(1f, 0.4f, 1f, 1f);
        [Tooltip("How much of the tint is mixed into the block's own colour (0 = none, 1 = flat tint).")]
        [SerializeField, Range(0f, 1f)] private float _tintStrength = 0.35f;

        public BiomeBlock Slot => _slot;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData)
        {
            TileBase preview = _previewBiome != null ? _previewBiome.Get(_slot)?.Tile : null;
            if (preview != null && preview != this) preview.GetTileData(position, tilemap, ref tileData);

            // Block tiles get their colour from Tile.color (the sprite is a white pixel), so mix the tint in
            // instead of replacing it - otherwise every biome block would look the same.
            tileData.color = Color.Lerp(tileData.color, _tint, _tintStrength);
            tileData.flags |= TileFlags.LockColor;
            tileData.colliderType = Tile.ColliderType.None;
        }
    }
}
