using UnityEngine;
using Wordania.Features.Bosses.Data;

namespace Wordania.Features.Journal.Entries
{
    [CreateAssetMenu(fileName = "NewBossEntry", menuName = "Journal/Boss")]
    public sealed class JournalBossEntry : JournalEntry<BossTemplate>
    {
        public override JournalCategory Category => JournalCategory.Bosses;
    }

}