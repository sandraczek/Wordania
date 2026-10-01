using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;
using Wordania.Services;
using Wordania.Data;
namespace Wordania.World.Data
{
    [CreateAssetMenu(fileName = "BlockRegistry", menuName = "World/Block Registry")]
    public sealed class BlockRegistry : AssetRegistry<BlockData>, IBlockRegistry
    {
        [SerializeField] TileBase[] _crackTiles;
        public TileBase GetCracks(float damage)
        {
            if (damage >= 1f) return null;

            int index = Mathf.FloorToInt((damage + 0.01f) * (_crackTiles.Length + 1));
            index = Mathf.Clamp(index, 0, _crackTiles.Length);
            if (index == 0) return null;

            return _crackTiles[index - 1];
        }
    }
}