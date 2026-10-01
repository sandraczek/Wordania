#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.Journal.Editor;
using Wordania.Journal.Entries;
using Wordania.World.Data;

namespace Wordania.World.Editor
{
    [CustomEditor(typeof(BlockData))]
    public sealed class BlockDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Create Journal Entry", GUILayout.Height(30)))
            {
                JournalEntryFactory.CreateOrSelect<JournalBlockEntry>(
                    (BlockData)target, "Blocks");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
