using System.Collections.Generic;
using Wordania.Constants;
using Wordania.Identifiers;
using Wordania.World.Events;

namespace Wordania.Journal
{
    public interface IPlayerJournal
    {
        int Increment(JournalCategory category, AssetId id);
        void IncrementBatch(JournalCategory category, IReadOnlyList<BlockMineRecord> minedBlocks);
        IReadOnlyDictionary<AssetId, int> GetDictionary(JournalCategory category);
        public void SetInitial(Dictionary<AssetId, int>[] categories);
    }
}