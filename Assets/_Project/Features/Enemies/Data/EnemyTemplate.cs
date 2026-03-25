using System;
using UnityEngine;
using Wordania.Gameplay.Enemies.Core;
using Wordania.Gameplay.Inventory;
using Wordania.Gameplay.Movement;

namespace Wordania.Gameplay.Enemies.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Data")]
    public sealed class EnemyTemplate : ScriptableObject
    {
        [field: Header("Prefab")]
        [field: SerializeField] public EnemyController Prefab;

        [field: Header("Identity")]
        [field: SerializeField] public string EnemyId { get; private set; } = Guid.NewGuid().ToString();
        
        [field: SerializeField] public string DisplayName { get; private set; } = "Unknown Enemy";

        [field: Space(10)]
        [field: Header("Modules")]
        [field: SerializeField] public EnemyStatsData Stats { get; private set; }
        [field: SerializeField] public EnemyMovementData Movement { get; private set; }
        [field: SerializeField] public EnemyCombatData Combat { get; private set; }
        [field: SerializeField] public EnemySpawnData Spawn { get; private set; }
        
        [field: SerializeField] public ItemData Loot { get; private set; }
        
        //to change
        public float FallDamageThreshold => Movement.FallDamageThreshold;
        public float FallDamageMultiplier => Movement.FallDamageMultiplier;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            CalculateClearanceFromPrefab();

            if (Combat != null)
            {
                if (Combat.LoseTargetRadius < Combat.DetectionRadius)
                {
                    Debug.LogWarning($"[{DisplayName}] LoseTargetRadius cannot be lesser than DetectionRadius! Fixing automatically.");
                    Combat.ForceValidTargetRadius();
                }
            }
        }

        private void CalculateClearanceFromPrefab()
        {
            if (Prefab == null)
            {
                return;
            }

            if(Spawn == null)
            {
                Debug.LogWarning($"[{DisplayName}] Spawn property is null", this);
            }

            if (!Prefab.TryGetComponent(out Collider2D col))
            {
                Debug.LogWarning($"[{nameof(DisplayName)}] Prefab {Prefab.name} is missing a Collider2D!", this);
            }
            Spawn.RequiredClearanceSize = col switch
            {
                BoxCollider2D box => box.size,
                CapsuleCollider2D capsule => capsule.size,
                CircleCollider2D circle => Vector2.one * (circle.radius * 2f),
                _ => Vector2.zero
            };
        }
        public void DrawSpawnGizmo(Vector2 position)
        {
            // Dzięki temu w edytorze zobaczysz różowy prostokąt w miejscu, 
            // gdzie system "myśli", że wróg się zmieści.
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(position, Spawn.RequiredClearanceSize);
        }
        #endif
    }
}