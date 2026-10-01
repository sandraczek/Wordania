#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.Enemies.Data;
using Wordania.Journal.Editor;
using Wordania.Journal.Entries;

namespace Wordania.Enemies.Editor
{
    [CustomEditor(typeof(EnemyTemplate))]
    public sealed class EnemyTemplateEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Create Journal Entry", GUILayout.Height(30)))
            {
                JournalEntryFactory.CreateOrSelect<JournalEnemyEntry>(
                    (EnemyTemplate)target, "Enemies");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
