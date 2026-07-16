using UnityEngine;
using Wordania.Features.Bosses.Data;

namespace Wordania.Features.Journal.Entries
{
    [CreateAssetMenu(fileName = "NewBossEntry", menuName = "Journal/Boss")]
    public sealed class JournalBossEntry : JournalEntry
    {
        [SerializeField] private BossTemplate _boss;

        public override JournalCategory Category => JournalCategory.Bosses;

    }
}