using UnityEngine;

namespace Wordania.Encounters
{
    /// <summary>
    /// A named area of the world (burning pyre, boss arena, trap room...). Pure geometry: anyone can ask
    /// whether a point is inside. Shape = any Collider2D on this object (set as trigger automatically).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ZoneArea : MonoBehaviour
    {
        [SerializeField] private string _zoneName = "Zone";
        [SerializeField] private Color _gizmoColor = new(1f, 0.45f, 0f, 0.35f);

        private Collider2D _area;

        public string ZoneName => _zoneName;

        private void Awake()
        {
            _area = GetComponent<Collider2D>();
            _area.isTrigger = true;
        }

        public bool Contains(Vector2 point) => _area != null && _area.OverlapPoint(point);

        private void OnDrawGizmos()
        {
            if (!TryGetComponent(out Collider2D col)) return;
            Gizmos.color = _gizmoColor;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}
