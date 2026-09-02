using System.Collections.Generic;
using Wordania.Core.Constants;
using Wordania.Core.Identifiers;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.Journal
{
    public interface IJournalService
    {
        public IReadOnlyDictionary<AssetId, int> GetDictionary(PersistentId persistentId, JournalCategory category);
        public int GetKilled(PersistentId persistentId, JournalCategory category, AssetId id);
        public int GetKilled(PersistentId persistentId, JournalEntry entry);
    }
}