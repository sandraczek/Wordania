#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.Features.Bosses.Data;
using Wordania.Features.Journal.Editor;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.Bosses.Editor
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
                    (BossTemplate)target, "Bosses", "_boss");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
