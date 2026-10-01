using System.Collections.Generic;
using Wordania.Constants;
using Wordania.Data;

namespace Wordania.Journal.Entries
{
    public interface IJournalEntryRegistry : IAssetRegistry<JournalEntry>
    {
        public List<JournalBossEntry> Bosses { get; }
        public List<JournalEnemyEntry> Enemies { get; }
        public List<JournalBlockEntry> Blocks { get; }

        public int Count(JournalCategory category);
    }
}