using System.Collections.Generic;
using Wordania.Core.Constants;
using Wordania.Core.Identifiers;
using Wordania.Features.Journal.Entries;

namespace Wordania.Features.Journal
{
    public interface IJournalService
    {
        public IReadOnlyDictionary<AssetId, int> GetDictionary(JournalCategory category);
        public int GetKilled(JournalCategory category, AssetId id);
        public int GetKilled(JournalEntry entry);
    }
}