using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wordania.Gameplay.World
{
    [CreateAssetMenu(fileName = "BlockDatabase", menuName = "World/Block Database")]
    public sealed class BlockDatabase : ScriptableObject, IBlockDatabase
    {
        [SerializeField]
        private List<BlockData> allBlocks = new();
        private Dictionary<int, BlockData> _blockMap;
        [SerializeField] TileBase[] _crackTiles;

        public void Initialize() // TO BE REPLACED WITH BELOW
        {
            _blockMap = new Dictionary<int, BlockData>();
            foreach (var block in allBlocks)
            {
                if (block != null) _blockMap.TryAdd(block.ID, block);
            }
        }
    //     public void Initialize()
    // {
    //     // Senior Level Optimization: 
    //     // Podajemy 'Capacity' do słownika, aby uniknąć kosztownych operacji 'Resize' i rehashowania.
    //     _blockMap = new Dictionary<int, BlockData>(allBlocks.Count);

    //     foreach (var block in allBlocks)
    //     {
    //         if (block == null) continue;

    //         // Używamy TryAdd lub jawnego sprawdzenia, aby uniknąć wyjątków 
    //         // przy duplikacji ID w edytorze (częsty błąd przy SO).
    //         if (!_blockMap.TryAdd(block.ID, block))
    //         {
    //             Debug.LogWarning($"[BlockDatabase] Duplicated ID: {block.ID} for block {block.name}.");
    //         }
    //     }
    // }

        public BlockData GetBlock(int id)
        {
            if(id==0) return null;
            if (_blockMap.TryGetValue(id, out var block)) return block;
            else Debug.LogError("No id " + id + " in block database");
            return null;
        }
        public TileBase GetCracks(float damage)
        {
            if (damage >= 1f) return null;

            int index = Mathf.FloorToInt((damage + 0.01f)* (_crackTiles.Length + 1 ));
            index = Mathf.Clamp(index, 0, _crackTiles.Length);
            if(index == 0) return null;

            return _crackTiles[index - 1];
        }

        #if UNITY_EDITOR
        [ContextMenu("Auto-Find All Blocks")]
        public void FindAllBlocksInProject()
        {
            if(allBlocks== null)
            {
                allBlocks = new();
            }
            allBlocks.Clear();

            string[] guids = AssetDatabase.FindAssets("t:BlockData");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BlockData block = AssetDatabase.LoadAssetAtPath<BlockData>(path);
                
                if (block != null)
                {
                    allBlocks.Add(block);
                }
            }
            
            Debug.Log($"Success! Found and added {allBlocks.Count} blocks to database.");
            
            EditorUtility.SetDirty(this); 
        }
        #endif
    }
}