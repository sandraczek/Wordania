using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wordania.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "ItemDataBase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject, IItemDatabase
    {
        [SerializeField]
        private List<ItemData> allItems = new();
        private Dictionary<string, ItemData> _itemMap;

        public void Initialize() // to do - upgrade like block database
        {
            _itemMap = new Dictionary<string, ItemData>();
            foreach (var item in allItems)
            {
                if (item != null) _itemMap.TryAdd(item.Id, item);
            }
        }
        public ItemData GetItem(string id)
        {
            if(id=="") return null;
            if (_itemMap.TryGetValue(id, out var item)) return item;
            else Debug.LogError("No id " + id + "in item database");
            return null;
        }

        #if UNITY_EDITOR
        [ContextMenu("Auto-Find All Items")]
        public void FindAllItemsInProject()
        {
            allItems.Clear();

            string[] guids = AssetDatabase.FindAssets("t:ItemData");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                
                if (item != null)
                {
                    allItems.Add(item);
                }
            }
            
            Debug.Log($"Success! Found and added {allItems.Count} items to database.");
            
            EditorUtility.SetDirty(this); 
        }
        #endif
    }
}