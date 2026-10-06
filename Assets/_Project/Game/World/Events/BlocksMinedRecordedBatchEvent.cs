using System.Collections.Generic;
using UnityEngine;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.World.Events
{

    public readonly struct BlockMineRecordedRecord
    {
        public readonly AssetId Id;
        public readonly int PreviousCount;
        public readonly int CurrentCount;

        public BlockMineRecordedRecord(AssetId blockAssetId, int previousCount, int currentCount)
        {
            Id = blockAssetId;
            PreviousCount = previousCount;
            CurrentCount = currentCount;
        }
    }
    public readonly struct BlocksMinedRecordedBatchEvent : ISimulationEvent
    {
        public readonly PersistentId PersistentId;

        public readonly IReadOnlyList<BlockMineRecordedRecord> MinedBlocks;

        public BlocksMinedRecordedBatchEvent(PersistentId persistentId, IReadOnlyList<BlockMineRecordedRecord> minedBlocks)
        {
            PersistentId = persistentId;
            MinedBlocks = minedBlocks;
        }
    }
}