using System.Collections.Generic;
using Wordania.Core.Constants;
using Wordania.Core.Identifiers;
using Wordania.Features.Journal.Entries;
using Wordania.Features.World.Events;

namespace Wordania.Features.Journal
{
    public interface IPlayerJournal
    {
        int Increment(JournalCategory category, AssetId id);
        void IncrementBatch(JournalCategory category, IReadOnlyList<BlockMineRecord> minedBlocks);
        IReadOnlyDictionary<AssetId, int> GetDictionary(JournalCategory category);
        public void SetInitial(Dictionary<AssetId, int>[] categories);
    }
}