#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.Structures.Authoring;

namespace Wordania.Structures.Editor
{
    [CustomEditor(typeof(StructureAuthoring))]
    public sealed class StructureAuthoringEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Paint Main / Background with the blocks' own tiles (Tile Palette).\n" +
                "StructureAir = carve air, empty cell = keep the terrain.\n" +
                "Markers (zones, player spawn) and enemy spawns are child objects.\n" +
                "Bakes into the template every time the prefab is saved.", MessageType.Info);

            DrawDefaultInspector();

            var authoring = (StructureAuthoring)target;
            EditorGUILayout.Space();

            if (GUILayout.Button("Bake now")) StructureBaker.Bake(authoring);

            if (EditorUtility.IsPersistent(authoring))
            {
                EditorGUILayout.HelpBox("Open the prefab to add markers and enemies.", MessageType.None);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add marker")) Add<StructureMarker>(authoring, "Markers", "Zone");
                if (GUILayout.Button("Add enemy spawn")) Add<StructureEnemySpawn>(authoring, "Enemies", "Enemy");
            }
        }

        /// <summary>New child in the group, in the cell at the Scene view's center.</summary>
        private static void Add<T>(StructureAuthoring authoring, string group, string name) where T : Component
        {
            Transform parent = authoring.transform.Find(group);
            if (parent == null)
            {
                parent = new GameObject(group).transform;
                parent.SetParent(authoring.transform, false);
                Undo.RegisterCreatedObjectUndo(parent.gameObject, $"Add {group}");
            }

            Vector3 pivot = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : authoring.transform.position;
            Vector2Int cell = authoring.ToCell(pivot);

            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.position = authoring.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
            child.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(child, $"Add {name}");

            Selection.activeGameObject = child;
        }
    }
}
#endif
