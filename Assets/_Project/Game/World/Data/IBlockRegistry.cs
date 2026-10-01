using UnityEngine;
using UnityEngine.Tilemaps;
using Wordania.Data;

namespace Wordania.World.Data
{
    public interface IBlockRegistry : IAssetRegistry<BlockData>
    {
        public TileBase GetCracks(float damage);
    }
}