using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Events
{
    public struct EnemyKillRecordedEvent : ISimulationEvent
    {
        public PersistentId PersistentId;
        public AssetId EnemyId;
        public int KillCount;
    }
}