using UnityEngine;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Bosses.Events
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