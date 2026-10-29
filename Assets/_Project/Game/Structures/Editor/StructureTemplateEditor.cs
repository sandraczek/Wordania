#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wordania.Structures.Editor
{
    /// <summary>Inspector preview of the ASCII rows: blocks, air, kept terrain, markers and guard zones.</summary>
    [CustomEditor(typeof(StructureTemplate))]
    public sealed class StructureTemplateEditor : UnityEditor.Editor
    {
        private static readonly Color KeepColor = new(0f, 0f, 0f, 0.15f);
        private static readonly Color AirColor = new(0.55f, 0.75f, 0.95f, 0.35f);
        private static readonly Color BlockFallbackColor = new(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color ZoneColor = new(0.2f, 1f, 0.3f, 1f);
        private static readonly Color ZoneFill = new(0.2f, 1f, 0.3f, 0.12f);

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var template = (StructureTemplate)target;
            if (template.Width == 0 || template.Height == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Preview  {template.Width} x {template.Height}  (x → , y ↑ from bottom-left)", EditorStyles.boldLabel);

            float cell = Mathf.Clamp((EditorGUIUtility.currentViewWidth - 40f) / template.Width, 6f, 18f);
            Rect area = GUILayoutUtility.GetRect(template.Width * cell, template.Height * cell, GUILayout.ExpandWidth(false));

            var labelStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter };

            for (int y = 0; y < template.Height; y++)
            {
                for (int x = 0; x < template.Width; x++)
                {
                    Rect r = CellRect(area, cell, template.Height, x, y);
                    char symbol = template.GetSymbol(x, y);
                    template.TryGetLegend(symbol, out StructureTemplate.LegendEntry entry);

                    EditorGUI.DrawRect(r, GetColor(template.GetCell(x, y), entry));
                    if (!string.IsNullOrEmpty(entry.Marker)) GUI.Label(r, symbol.ToString(), labelStyle);
                }
            }

            DrawZones(template, area, cell);
        }

        private static Color GetColor(StructureTemplate.Cell cell, StructureTemplate.LegendEntry entry)
        {
            switch (cell.Kind)
            {
                case StructureTemplate.CellKind.Air: return AirColor;
                case StructureTemplate.CellKind.Block:
                    return entry.Main != null && entry.Main.MapColor.a > 0 ? (Color)entry.Main.MapColor : BlockFallbackColor;
                default: return KeepColor;
            }
        }

        private static void DrawZones(StructureTemplate template, Rect area, float cell)
        {
            var drawn = new HashSet<string>();
            foreach (StructureTemplate.EnemySlot slot in template.Enemies)
            {
                if (string.IsNullOrEmpty(slot.GuardZoneMarker) || !drawn.Add(slot.GuardZoneMarker)) continue;
                if (!template.TryGetMarkerBounds(slot.GuardZoneMarker, out RectInt bounds)) continue;

                Rect min = CellRect(area, cell, template.Height, bounds.xMin, bounds.yMax - 1);
                var zone = new Rect(min.x, min.y, bounds.width * cell, bounds.height * cell);
                Handles.DrawSolidRectangleWithOutline(zone, ZoneFill, ZoneColor);
            }
        }

        /// <summary>GUI y grows downwards, structure y grows upwards.</summary>
        private static Rect CellRect(Rect area, float cell, int height, int x, int y) =>
            new(area.x + x * cell, area.y + (height - 1 - y) * cell, cell - 1f, cell - 1f);
    }
}
#endif
