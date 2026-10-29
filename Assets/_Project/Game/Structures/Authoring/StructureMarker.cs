using UnityEngine;

namespace Wordania.Structures.Authoring
{
    /// <summary>
    /// Named area of a structure (guard zone, player spawn, ...). Name = GameObject name (unique per structure).
    /// The bottom-left cell is the cell under the object's position, <see cref="Size"/> extends right and up.
    /// </summary>
    public sealed class StructureMarker : MonoBehaviour
    {
        [SerializeField] private Vector2Int _size = Vector2Int.one;
        [SerializeField] private Color _color = new(0.2f, 1f, 0.3f, 1f);

        public Vector2Int Size => Vector2Int.Max(_size, Vector2Int.one);

#if UNITY_EDITOR
        public void EditorInitialize(Vector2Int size, Color color)
        {
            _size = size;
            _color = color;
        }

        private void OnDrawGizmos()
        {
            var authoring = GetComponentInParent<StructureAuthoring>();
            if (authoring == null) return;

            Vector2Int cell = authoring.ToCell(transform.position);
            Vector3 min = authoring.CellToWorld(cell);
            Vector3 max = authoring.CellToWorld(cell + Size);

            var fill = new Color(_color.r, _color.g, _color.b, 0.12f);
            UnityEditor.Handles.DrawSolidRectangleWithOutline(new Rect(min, max - min), fill, _color);
            UnityEditor.Handles.Label(new Vector3(min.x, max.y), name, LabelStyle(_color));
        }

        internal static GUIStyle LabelStyle(Color color)
        {
            var style = new GUIStyle(UnityEditor.EditorStyles.miniBoldLabel);
            style.normal.textColor = color;
            return style;
        }
#endif
    }
}
