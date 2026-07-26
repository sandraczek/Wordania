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
        private const string SourceFieldName = "_source";

        /// <param name="source">The template the entry describes (enemy/block/boss).</param>
        /// <param name="subFolder">Sub-folder under Definitions, e.g. "Enemies".</param>
        public static void CreateOrSelect<TEntry>(
            DataAsset source, string subFolder)
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

            string entryEditorId = $"{sourceEditorId}_journal_entry";
            string entryFileName = $"{source.name}_JournalEntry";

            // Reuse an existing entry that already points at this source OR that already
            // has the same asset file name (treated as the entry to modify):
            // refresh its source reference and id, but keep its milestones intact.
            if (TryFindExisting<TEntry>(source, entryFileName, out TEntry existing))
            {
                var existingSo = new SerializedObject(existing);
                existingSo.FindProperty(SourceFieldName).objectReferenceValue = source;
                existingSo.FindProperty("_id._editorId").stringValue = entryEditorId;
                existingSo.ApplyModifiedProperties();

                AssetDatabase.SaveAssets();

                EditorGUIUtility.PingObject(existing);
                Selection.activeObject = existing;
                Debug.Log(
                    $"[Journal] Refreshed existing entry '{existing.name}' " +
                    $"(source and id updated, milestones kept) for '{source.name}'.",
                    existing);
                return;
            }

            string folder = EnsureFolder(subFolder);
            string entryPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, $"{entryFileName}.asset"));

            var entry = ScriptableObject.CreateInstance<TEntry>();
            AssetDatabase.CreateAsset(entry, entryPath);

            var entrySo = new SerializedObject(entry);
            entrySo.FindProperty(SourceFieldName).objectReferenceValue = source;
            entrySo.FindProperty("_id._editorId").stringValue = entryEditorId;
            entrySo.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();

            EditorGUIUtility.PingObject(entry);
            Selection.activeObject = entry;

            Debug.Log(
                $"[Journal] Created entry '{entry.name}' with id " +
                $"'{entryEditorId}' for '{source.name}'.", entry);
        }

        private static string ReadEditorId(DataAsset source)
        {
            var so = new SerializedObject(source);
            return so.FindProperty("_id._editorId").stringValue;
        }

        private static bool TryFindExisting<TEntry>(
            DataAsset source, string entryFileName, out TEntry existing)
            where TEntry : JournalEntry
        {
            existing = null;
            TEntry nameMatch = null;

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(TEntry).Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var candidate = AssetDatabase.LoadAssetAtPath<TEntry>(path);
                if (candidate == null) continue;

                var so = new SerializedObject(candidate);
                if (so.FindProperty(SourceFieldName).objectReferenceValue == source)
                {
                    // Source-reference match takes priority.
                    existing = candidate;
                    return true;
                }

                if (nameMatch == null && candidate.name == entryFileName)
                {
                    nameMatch = candidate;
                }
            }

            existing = nameMatch;
            return existing != null;
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
