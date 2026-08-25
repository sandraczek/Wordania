using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Features.World.Events
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
    public readonly struct BlocksMinedRecordedBatchEvent : IGameEvent
    {
        public readonly InstanceId InstigatorEntityId;

        public readonly IReadOnlyList<BlockMineRecordedRecord> MinedBlocks;

        public BlocksMinedRecordedBatchEvent(InstanceId instigatorEntityId, IReadOnlyList<BlockMineRecordedRecord> minedBlocks)
        {
            InstigatorEntityId = instigatorEntityId;
            MinedBlocks = minedBlocks;
        }
    }
}