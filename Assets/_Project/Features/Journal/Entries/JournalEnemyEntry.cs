using UnityEngine;
using Wordania.Features.Enemies.Data;

namespace Wordania.Features.Journal.Entries
{
    [CreateAssetMenu(fileName = "NewEnemyEntry", menuName = "Journal/Enemy")]
    public sealed class JournalEnemyEntry : JournalEntry
    {
        [SerializeField] private EnemyTemplate _enemy;

        public override JournalCategory Category => JournalCategory.Enemies;

    }
}