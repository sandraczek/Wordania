using System.Collections.Generic;
using Wordania.Constants;
using Wordania.Identifiers;
using Wordania.Journal.Entries;

namespace Wordania.Journal
{
    public interface IJournalService
    {
        public IReadOnlyDictionary<AssetId, int> GetDictionary(PersistentId persistentId, JournalCategory category);
        public int GetKilled(PersistentId persistentId, JournalCategory category, AssetId id);
        public int GetKilled(PersistentId persistentId, JournalEntry entry);
    }
}