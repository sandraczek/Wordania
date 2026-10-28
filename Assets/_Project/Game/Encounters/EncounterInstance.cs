using System;
using System.Collections.Generic;
using UnityEngine;
using Wordania.Enemies.AI;
using Wordania.Enemies.Core;
using Wordania.Enemies.Data;

namespace Wordania.Encounters
{
    /// <summary>
    /// A hand-authored set piece (prefab): scenery, zones and enemy slots. Unlike random spawns, its enemies
    /// get an <see cref="EnemySpawnContext"/> describing their place in the scene (e.g. which zone to guard).
    /// Host only.
    /// </summary>
    public sealed class EncounterInstance : MonoBehaviour
    {
        [Serializable]
        public struct EnemySlot
        {
            public EnemyTemplate Template;
            public Transform Point;
            [Tooltip("Drop the enemy onto the ground below/above the point (procedural terrain).")]
            public bool SnapToGround;
        }

        [Tooltip("Zone the encounter's enemies guard (passed as GuardZone).")]
        [SerializeField] private ZoneArea _guardZone;
        [SerializeField] private List<EnemySlot> _enemies = new();

        [Header("Ground Snapping")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField, Min(1f)] private float _groundSearchHeight = 20f;

        public ZoneArea GuardZone => _guardZone;

        public void Begin(IEnemyFactory factory)
        {
            var context = new EnemySpawnContext(_guardZone);

            foreach (EnemySlot slot in _enemies)
            {
                if (slot.Template == null || slot.Point == null)
                {
                    Debug.LogWarning($"[Encounter] {name}: empty enemy slot skipped", this);
                    continue;
                }

                Vector2 position = slot.Point.position;
                if (slot.SnapToGround) position = SnapToGround(position, slot.Template);

                factory.CreateEnemy(slot.Template, position, context);
            }
        }

        private Vector2 SnapToGround(Vector2 point, EnemyTemplate template)
        {
            Vector2 origin = point + Vector2.up * _groundSearchHeight;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _groundSearchHeight * 2f, _groundLayer);
            if (hit.collider == null) return point;

            float halfHeight = template.Spawn != null ? template.Spawn.RequiredClearanceSize.y * 0.5f : 0.5f;
            return new Vector2(point.x, hit.point.y + halfHeight + 0.05f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (EnemySlot slot in _enemies)
                if (slot.Point != null) Gizmos.DrawWireSphere(slot.Point.position, 0.4f);
        }
    }
}
