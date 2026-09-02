using System.Collections.Generic;
using UnityEngine;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Features.World.Events
{
    public readonly struct BlockMineRecord
    {
        public readonly AssetId Id;
        public readonly int Count;

        public BlockMineRecord(AssetId blockAssetId, int count)
        {
            Id = blockAssetId;
            Count = count;
        }
    }

    public readonly struct BlocksMinedBatchEvent : IGameEvent
    {
        public readonly InstanceId InstigatorId;

        public readonly IReadOnlyList<BlockMineRecord> MinedBlocks;

        public BlocksMinedBatchEvent(InstanceId instigatorEntityId, IReadOnlyList<BlockMineRecord> minedBlocks)
        {
            InstigatorId = instigatorEntityId;
            MinedBlocks = minedBlocks;
        }
    }
}