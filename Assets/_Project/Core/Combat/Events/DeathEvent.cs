using UnityEngine;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Combat.Events
{
    public struct DeathEvent : IGameEvent
    {
        public AssetId VictimAssetId;
        public int InstigatorEntityId;
        public DeathEvent(AssetId victimAssetId, int instigatorEntityId)
        {
            VictimAssetId = victimAssetId;
            InstigatorEntityId = instigatorEntityId;
        }
    }
}