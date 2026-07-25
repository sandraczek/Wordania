#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.Journal.Editor
{
    /// <summary>
    /// Editor helper that creates <see cref="JournalEntry"/> assets from a source
    /// <see cref="DataAsset"/> (enemy, block, boss...), enforcing one entry per source.
    /// </summary>
    public static class JournalEntryFactory
    {
        private const string BaseFolder = "Assets/_Project/Features/Journal/Definitions";

        /// <param name="source">The template the entry describes (enemy/block/boss).</param>
        /// <param name="subFolder">Sub-folder under Definitions, e.g. "Enemies".</param>
        /// <param name="sourceFieldName">Private serialized field on the entry pointing at the source.</param>
        public static void CreateOrSelect<TEntry>(
            DataAsset source, string subFolder, string sourceFieldName)
            where TEntry : JournalEntry
        {
            if (source == null) return;

            string sourceEditorId = ReadEditorId(source);
            if (string.IsNullOrEmpty(sourceEditorId))
            {
                EditorUtility.DisplayDialog(
                    "Missing AssetId",
                    $"'{source.name}' has no AssetId set. Assign one before creating a journal entry.",
                    "OK");
                return;
            }

            // Prevent duplicates: reuse an existing entry that already points at this source.
            if (TryFindExisting<TEntry>(sourceFieldName, source, out TEntry existing))
            {
                EditorGUIUtility.PingObject(existing);
                Selection.activeObject = existing;
                Debug.Log(
                    $"[Journal] '{source.name}' already has a journal entry '{existing.name}'.",
                    existing);
                return;
            }

            string folder = EnsureFolder(subFolder);
            string entryPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, $"{source.name}_JournalEntry.asset"));

            var entry = ScriptableObject.CreateInstance<TEntry>();
            AssetDatabase.CreateAsset(entry, entryPath);

            var entrySo = new SerializedObject(entry);
            entrySo.FindProperty(sourceFieldName).objectReferenceValue = source;
            entrySo.FindProperty("_id._editorId").stringValue = $"{sourceEditorId}_journal_entry";
            entrySo.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();

            EditorGUIUtility.PingObject(entry);
            Selection.activeObject = entry;

            Debug.Log(
                $"[Journal] Created entry '{entry.name}' with id " +
                $"'{sourceEditorId}_journal' for '{source.name}'.", entry);
        }

        private static string ReadEditorId(DataAsset source)
        {
            var so = new SerializedObject(source);
            return so.FindProperty("_id._editorId").stringValue;
        }

        private static bool TryFindExisting<TEntry>(
            string sourceFieldName, DataAsset source, out TEntry existing)
            where TEntry : JournalEntry
        {
            existing = null;
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(TEntry).Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var candidate = AssetDatabase.LoadAssetAtPath<TEntry>(path);
                if (candidate == null) continue;

                var so = new SerializedObject(candidate);
                if (so.FindProperty(sourceFieldName).objectReferenceValue == source)
                {
                    existing = candidate;
                    return true;
                }
            }
            return false;
        }

        private static string EnsureFolder(string subFolder)
        {
            string full = $"{BaseFolder}/{subFolder}";
            if (AssetDatabase.IsValidFolder(full)) return full;

            // Create every missing segment of the path.
            string current = "Assets";
            foreach (string segment in full.Substring("Assets/".Length).Split('/'))
            {
                string next = $"{current}/{segment}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segment);
                }
                current = next;
            }
            return full;
        }
    }
}
#endif
