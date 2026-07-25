#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.Features.Enemies.Data;
using Wordania.Features.Journal.Editor;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.Enemies.Editor
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
                    (EnemyTemplate)target, "Enemies", "_enemy");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
