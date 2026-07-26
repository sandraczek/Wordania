using System;
using System.Linq;
using UnityEngine;
using Wordania.Core.Data;
using Wordania.Core.Identifiers;
using Wordania.Features.Enemies.Core;
using Wordania.Features.Inventory;
using Wordania.Features.Movement;
using Wordania.Features.Skills;

namespace Wordania.Features.Enemies.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Data")]
    public sealed class EnemyTemplate : DataAsset
    {
        [field: Header("Prefab")]
        [field: SerializeField] public EnemyController Prefab;

        [field: SerializeField] public string DisplayName { get; private set; } = "Unknown Enemy";

        [field: Space(10)]
        [field: Header("Modules")]
        [field: SerializeField] public EnemyStatsData Stats { get; private set; }
        [field: SerializeField] public EnemyMovementData Movement { get; private set; }
        [field: SerializeField] public EnemyCombatData Combat { get; private set; }
        [field: SerializeField] public EnemySpawnData Spawn { get; private set; }
        [field: SerializeField] public RewardData Reward { get; private set; }

        [field: SerializeField] public ItemData Loot { get; private set; }

        //to change
        public float FallDamageThreshold => Movement.FallDamageThreshold;
        public float FallDamageMultiplier => Movement.FallDamageMultiplier;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            CalculateClearanceFromPrefab();

            if (Combat != null)
            {
                if (Combat.LoseTargetRadius < Combat.DetectionRadius)
                {
                    Debug.LogWarning($"[{DisplayName}] LoseTargetRadius cannot be lesser than DetectionRadius!");
                }
            }

            Reward?.EditorSortThreshold();
        }

        private void CalculateClearanceFromPrefab()
        {
            if (Prefab == null)
            {
                return;
            }

            if (Spawn == null)
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
#endif
    }
}