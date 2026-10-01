using UnityEngine;
using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Events
{
    public struct DeathEvent : IGameEvent
    {
        public AssetId VictimAssetId;
        public InstanceId InstigatorId;
        public DeathEvent(AssetId victimAssetId, InstanceId instigatorEntityId)
        {
            VictimAssetId = victimAssetId;
            InstigatorId = instigatorEntityId;
        }
    }
}