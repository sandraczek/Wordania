#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.Bosses.Data;
using Wordania.Journal.Editor;
using Wordania.Journal.Entries;

namespace Wordania.Bosses.Editor
{
    [CustomEditor(typeof(BossTemplate), true)]
    public sealed class BossTemplateEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Create Journal Entry", GUILayout.Height(30)))
            {
                JournalEntryFactory.CreateOrSelect<JournalBossEntry>(
                    (BossTemplate)target, "Bosses");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
