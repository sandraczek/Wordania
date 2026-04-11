using UnityEngine;
using UnityEngine.Tilemaps;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Inventory;

namespace Wordania.Features.World
{
    [CreateAssetMenu(fileName = "NewBlock", menuName = "World/Block")]
    public sealed class BlockData : DataAsset
    {
        [Header("Visual")]
        public TileBase Tile;
        public Color32 MapColor = new(0, 0, 0, 0);

        [Header("Stats")]
        public float Hardness = 1;

        [Header("Item Info")]
        public Recipe recipe;
        public ItemData loot;
        public int lootAmount;
    }
}