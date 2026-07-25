using UnityEngine;
using Wordania.Core.Identifiers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wordania.Core.Data
{
    public abstract class DataAsset : ScriptableObject
    {
        [SerializeField] private AssetId _id;
        public AssetId Id => _id;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _id.EditorInitialize();

            // if (_id.IsEmpty)
            // {
            //     Debug.LogWarning($"[{GetType().Name}] Asset '{name}' has an empty AssetId. Assign a unique identifier.", this);
            //     return;
            // }

            // if (TryFindDuplicate(_id, out DataAsset other))
            // {
            //     Debug.LogError(
            //         $"[{GetType().Name}] Duplicate AssetId '{_id}' on '{name}'. " +
            //         $"Already used by '{other.name}'. IDs must be unique.", this);
            // }
        }

        private bool TryFindDuplicate(AssetId id, out DataAsset conflicting)
        {
            conflicting = null;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(DataAsset)}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<DataAsset>(path);

                if (asset == null || asset == this) continue;

                if (asset._id == id)
                {
                    conflicting = asset;
                    return true;
                }
            }

            return false;
        }

        [MenuItem("Tools/Validate Asset IDs")]
        private static void ValidateAllIds()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(DataAsset)}");
            var seen = new System.Collections.Generic.Dictionary<AssetId, DataAsset>(guids.Length);

            int emptyCount = 0;
            int duplicateCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<DataAsset>(path);
                if (asset == null) continue;

                if (asset._id.IsEmpty)
                {
                    Debug.LogWarning($"[{asset.GetType().Name}] Asset '{asset.name}' has an empty AssetId.", asset);
                    emptyCount++;
                    continue;
                }

                if (seen.TryGetValue(asset._id, out DataAsset other))
                {
                    Debug.LogError(
                        $"[{asset.GetType().Name}] Duplicate AssetId '{asset._id}' on '{asset.name}'. " +
                        $"Already used by '{other.name}'.", asset);
                    duplicateCount++;
                }
                else
                {
                    seen.Add(asset._id, asset);
                }
            }

            Debug.Log(
                $"[DataAsset] Validation complete. Scanned {guids.Length} assets — " +
                $"{emptyCount} empty, {duplicateCount} duplicate.");
        }
#endif
    }
}