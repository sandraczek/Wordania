#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.Features.World;
using Wordania.Features.Journal.Editor;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.World.Editor
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
                    (BlockData)target, "Blocks", "_block");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
