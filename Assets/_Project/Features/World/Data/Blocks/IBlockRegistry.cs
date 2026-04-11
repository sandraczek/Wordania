using UnityEngine;
using UnityEngine.Tilemaps;
using Wordania.Core.Data;

namespace Wordania.Features.World.Data
{
    public interface IBlockRegistry : IAssetRegistry<BlockData>
    {
        public TileBase GetCracks(float damage);
    }
}