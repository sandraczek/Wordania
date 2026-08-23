using UnityEngine;
using System.Collections.Generic;
using System;
using Wordania.Core.Identifiers;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wordania.Core.Data
{
    public abstract class AssetRegistry<T> : ScriptableObject, IAssetRegistry<T> where T : DataAsset
    {
        [SerializeField]
        private List<T> _assets = new();
        private Dictionary<AssetId, T> _lookupTable;
        public IReadOnlyList<T> Assets => _assets;

#if UNITY_EDITOR
        //debug
        private bool _showAvailableItems = true;
#endif
        protected virtual AssetId GetKey(T asset) => asset.Id;

        public virtual void Initialize()
        {
            _lookupTable = new Dictionary<AssetId, T>(_assets.Count);
            foreach (var asset in _assets)
            {
                if (asset == null) continue;
                if (!_lookupTable.TryAdd(GetKey(asset), asset))   // <- GetKey zamiast asset.Id
                    Debug.LogWarning($"[{typeof(T).Name}] Duplicated ID: {GetKey(asset)} for asset: {asset.name}.");
            }

#if UNITY_EDITOR
            //debug
            _showAvailableItems = false;
#endif
        }
        public T Get(AssetId id)
        {
            if (id.Hash == 0) return null;

            if (_lookupTable == null)
            {
                Debug.LogError($"[{GetType().Name}] Catalog not initialized. Call Initialize() first.");
                return null;
            }

            if (_lookupTable.TryGetValue(id, out var asset)) return asset;

            string dump = "Available hashes: ";
            int count = 0;
            foreach (var key in _lookupTable.Keys)
            {
                dump += $"{key.Hash} ({key}), ";
                if (++count >= 5) break; // dump up to 5 elements for brevity
            }

            Debug.LogError($"[{GetType().Name}] Asset with ID {id.Hash} was not found.");
#if UNITY_EDITOR
            if (_showAvailableItems) Debug.Log(dump);
#endif
            return null;
        }

#if UNITY_EDITOR
        public void Editor_FindAllAssets()
        {
            _assets ??= new();
            _assets.Clear();

            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                {
                    _assets.Add(asset);
                }
            }

            //Debug.Log($"Success! Found and added {_assets.Count} assets to {typeof(T).Name}.");

            EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}