using UnityEngine;
using UnityEngine.Tilemaps;
using Wordania.Data;
using Wordania.Inventory;

namespace Wordania.World.Data
{
    [CreateAssetMenu(fileName = "NewBlock", menuName = "World/Block")]
    public sealed class BlockData : DataAsset
    {
        [Header("Visual")]
        public TileBase Tile;
        public Color32 MapColor = new(0, 0, 0, 0);

        [Header("Stats")]
        public float Hardness = 1;

        [Header("Lighting")]
        [Tooltip("Amount of light emitted by this block (0 = none, 15 = max).")]
        [SerializeField, Range(0, 31)] private byte _lightEmission = 0;
        public byte LightEmission => _lightEmission;

        [Tooltip("How much light is subtracted when passing through this block (0 = transparent, 15 = fully opaque).")]
        [SerializeField, Range(0, 31)] private byte _lightOpacity = 15;
        public byte LightOpacity => _lightOpacity;

        [Header("Item Info")]
        public Recipe recipe;
        public ItemData loot;
        public int lootAmount;
    }
}