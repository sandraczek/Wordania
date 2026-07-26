using UnityEngine;
using Wordania.Core.Identifiers;
using Wordania.Features.Enemies.Data;

namespace Wordania.Features.Journal.Entries
{
    [CreateAssetMenu(fileName = "NewEnemyEntry", menuName = "Journal/Enemy")]
    public sealed class JournalEnemyEntry : JournalEntry<EnemyTemplate>
    {
        public override JournalCategory Category => JournalCategory.Enemies;
    }

}