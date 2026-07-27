using UnityEngine;
using Wordania.Core.Events;
using Wordania.Core.Identifiers;

namespace Wordania.Core.Combat.Events
{
    public struct DeathEvent : IGameEvent
    {
        public AssetId VictimAssetId;
        public InstanceId InstigatorEntityId;
        public DeathEvent(AssetId victimAssetId, InstanceId instigatorEntityId)
        {
            VictimAssetId = victimAssetId;
            InstigatorEntityId = instigatorEntityId;
        }
    }
}