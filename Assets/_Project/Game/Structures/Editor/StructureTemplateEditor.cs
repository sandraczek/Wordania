#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Wordania.World.Data;

namespace Wordania.Structures.Editor
{
    /// <summary>
    /// Template inspector: settings, link to the authoring prefab (the only place the shape is edited) and a
    /// read-only preview of the baked data - blocks, air, kept terrain, markers and enemies.
    /// </summary>
    [CustomEditor(typeof(StructureTemplate))]
    public sealed class StructureTemplateEditor : UnityEditor.Editor
    {
        private static readonly Color KeepColor = new(0f, 0f, 0f, 0.15f);
        private static readonly Color AirColor = new(0.55f, 0.75f, 0.95f, 0.35f);
        private static readonly Color BlockFallbackColor = new(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color EnemyColor = new(1f, 0.3f, 0.25f, 1f);

        private GameObject _authoring;

        private void OnEnable() => _authoring = StructureBaker.FindAuthoringPrefab((StructureTemplate)target);

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var template = (StructureTemplate)target;
            EditorGUILayout.Space();
            DrawAuthoringLink(template);

            if (template.Width == 0 || template.Height == 0)
            {
                EditorGUILayout.HelpBox("Nothing baked yet: create the authoring prefab, paint the structure and save it.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Baked  {template.Width} x {template.Height}  (x → , y ↑ from bottom-left)", EditorStyles.boldLabel);
            DrawPreview(template);
            DrawEnemyList(template);
        }

        private void DrawAuthoringLink(StructureTemplate template)
        {
            if (_authoring != null)
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField("Authoring Prefab", _authoring, typeof(GameObject), false);
                if (GUILayout.Button("Edit structure")) AssetDatabase.OpenAsset(_authoring);
                return;
            }

            if (!GUILayout.Button("Create authoring prefab")) return;

            _authoring = StructureBaker.CreateAuthoringPrefab(template);
            if (_authoring != null) AssetDatabase.OpenAsset(_authoring);
            GUIUtility.ExitGUI();
        }

        private static void DrawPreview(StructureTemplate template)
        {
            float cell = Mathf.Clamp((EditorGUIUtility.currentViewWidth - 40f) / template.Width, 6f, 18f);
            Rect area = GUILayoutUtility.GetRect(template.Width * cell, template.Height * cell, GUILayout.ExpandWidth(false));

            for (int y = 0; y < template.Height; y++)
                for (int x = 0; x < template.Width; x++)
                    EditorGUI.DrawRect(CellRect(area, cell, template.Height, x, y), GetColor(template, x, y));

            var labelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            foreach (StructureTemplate.Marker marker in template.Markers)
            {
                Color color = marker.Name == template.PlayerSpawnMarker ? StructureBaker.PlayerSpawnColor : StructureBaker.ZoneColor;
                Rect rect = AreaRect(area, cell, template.Height, marker.Area);
                Handles.DrawSolidRectangleWithOutline(rect, new Color(color.r, color.g, color.b, 0.12f), color);

                labelStyle.normal.textColor = color;
                GUI.Label(new Rect(rect.x + 2f, rect.y, 200f, 16f), marker.Name, labelStyle);
            }

            foreach (StructureTemplate.EnemySlot slot in template.Enemies)
            {
                Rect rect = CellRect(area, cell, template.Height, slot.Cell.x, slot.Cell.y);
                EditorGUI.DrawRect(new Rect(rect.center - rect.size * 0.3f, rect.size * 0.6f), EnemyColor);
            }
        }

        private static void DrawEnemyList(StructureTemplate template)
        {
            if (template.Enemies.Count == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Enemies", EditorStyles.boldLabel);
            for (int i = 0; i < template.Enemies.Count; i++)
            {
                StructureTemplate.EnemySlot slot = template.Enemies[i];
                string enemy = slot.Enemy != null ? slot.Enemy.name : "(none)";
                string zone = string.IsNullOrEmpty(slot.GuardZoneMarker) ? "-" : slot.GuardZoneMarker;
                string behaviour = slot.Behaviour != null ? slot.Behaviour.name : "default";
                EditorGUILayout.LabelField($"{i}. {enemy} at {slot.Cell}", $"zone: {zone}, behaviour: {behaviour}");
            }
        }

        private static Color GetColor(StructureTemplate template, int x, int y)
        {
            BlockData wall = template.GetBackgroundBlock(x, y);
            switch (template.GetCell(x, y).Kind)
            {
                case StructureTemplate.CellKind.Block: return MapColor(template.GetMainBlock(x, y));
                case StructureTemplate.CellKind.Air: return wall != null ? MapColor(wall) * 0.6f : AirColor;
                default: return wall != null ? MapColor(wall) * 0.6f : KeepColor;
            }
        }

        private static Color MapColor(BlockData block)
        {
            Color color = block != null && block.MapColor.a > 0 ? (Color)block.MapColor : BlockFallbackColor;
            color.a = 1f;
            return color;
        }

        /// <summary>GUI y grows downwards, structure y grows upwards.</summary>
        private static Rect CellRect(Rect area, float cell, int height, int x, int y) =>
            new(area.x + x * cell, area.y + (height - 1 - y) * cell, cell - 1f, cell - 1f);

        private static Rect AreaRect(Rect area, float cell, int height, RectInt cells)
        {
            Rect topLeft = CellRect(area, cell, height, cells.xMin, cells.yMax - 1);
            return new Rect(topLeft.x, topLeft.y, cells.width * cell, cells.height * cell);
        }
    }
}
#endif

