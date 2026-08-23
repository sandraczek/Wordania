using System.Collections.Generic;
using Wordania.Core.Data;

namespace Wordania.Features.Journal.Entries
{
    public interface IJournalEntryRegistry : IAssetRegistry<JournalEntry>
    {
        public List<JournalBossEntry> Bosses { get; }
        public List<JournalEnemyEntry> Enemies { get; }
        public List<JournalBlockEntry> Blocks { get; }

        public int Count(JournalCategory category);
    }
}