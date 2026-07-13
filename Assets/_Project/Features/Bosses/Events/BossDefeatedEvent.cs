using UnityEngine;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;
using Wordania.Features.Bosses.Core;

namespace Wordania.Features.Bosses.Events
{
    public struct BossDeathEvent : IGameEvent
    {
        public BossDeathEvent(AssetId assetId)
        {
            Id = assetId;
        }
        public AssetId Id;
    }
}