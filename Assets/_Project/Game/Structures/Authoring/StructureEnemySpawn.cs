using UnityEngine;
using Wordania.Enemies.AI;
using Wordania.Enemies.Data;

namespace Wordania.Structures.Authoring
{
    /// <summary>Enemy post of a structure: the enemy stands in the cell under the object's position.</summary>
    public sealed class StructureEnemySpawn : MonoBehaviour
    {
        private static readonly Color GizmoColor = new(1f, 0.3f, 0.25f, 1f);

        [SerializeField] private EnemyTemplate _enemy;
        [Tooltip("Zone the enemy guards (a marker of this structure). Empty = none.")]
        [SerializeField] private StructureMarker _guardZone;
        [Tooltip("Overrides the enemy's default behaviour. Empty = template default.")]
        [SerializeField] private EnemyBehaviourProfile _behaviour;

        public EnemyTemplate Enemy => _enemy;
        public StructureMarker GuardZone => _guardZone;
        public EnemyBehaviourProfile Behaviour => _behaviour;

#if UNITY_EDITOR
        public void EditorInitialize(EnemyTemplate enemy, StructureMarker guardZone, EnemyBehaviourProfile behaviour)
        {
            _enemy = enemy;
            _guardZone = guardZone;
            _behaviour = behaviour;
        }

        private void OnDrawGizmos()
        {
            var authoring = GetComponentInParent<StructureAuthoring>();
            if (authoring == null) return;

            Vector2Int cell = authoring.ToCell(transform.position);
            Vector3 cellMin = authoring.CellToWorld(cell);
            Vector3 cellMax = authoring.CellToWorld(cell + Vector2Int.one);
            var feet = new Vector3((cellMin.x + cellMax.x) * 0.5f, cellMin.y);

            Vector2 size = _enemy != null && _enemy.Spawn != null && _enemy.Spawn.RequiredClearanceSize != Vector2.zero
                ? _enemy.Spawn.RequiredClearanceSize
                : Vector2.one;

            Gizmos.color = GizmoColor;
            Gizmos.DrawWireCube(feet + Vector3.up * size.y * 0.5f, size);
            Gizmos.DrawSphere(feet, 0.1f);
            UnityEditor.Handles.Label(feet + Vector3.up * (size.y + 0.4f), _enemy != null ? _enemy.name : "(no enemy)",
                StructureMarker.LabelStyle(GizmoColor));

            if (_guardZone == null) return;

            Vector2Int zoneCell = authoring.ToCell(_guardZone.transform.position);
            Vector3 zoneMin = authoring.CellToWorld(zoneCell);
            Vector3 zoneMax = authoring.CellToWorld(zoneCell + _guardZone.Size);
            UnityEditor.Handles.color = GizmoColor;
            UnityEditor.Handles.DrawDottedLine(feet, (zoneMin + zoneMax) * 0.5f, 3f);
        }
#endif
    }
}
