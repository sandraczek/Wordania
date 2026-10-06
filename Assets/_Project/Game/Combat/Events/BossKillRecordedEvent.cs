using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Events
{
    public struct BossKillRecordedEvent : ISimulationEvent
    {
        public PersistentId PersistentId;
        public AssetId BossId;
        public int KillCount;
    }
}