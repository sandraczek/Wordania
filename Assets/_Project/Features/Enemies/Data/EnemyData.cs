using System;
using UnityEngine;
using Wordania.Gameplay.Enemies.Core;
using Wordania.Gameplay.Inventory;

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
        
        [field: SerializeField] public ItemData Loot { get; private set; }

        private void OnValidate()
        {
            if (Combat != null)
            {
                if (Combat.LoseTargetRadius < Combat.DetectionRadius)
                {
                    Debug.LogWarning($"[{DisplayName}] LoseTargetRadius cannot be lesser than DetectionRadius! Fixing automatically.");
                    Combat.ForceValidTargetRadius();
                }
            }
        }
    }
}