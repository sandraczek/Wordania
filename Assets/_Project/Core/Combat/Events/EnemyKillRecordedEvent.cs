using Wordania.Events;
using Wordania.Identifiers;

namespace Wordania.Combat.Events
{
    public struct EnemyKillRecordedEvent : IGameEvent
    {
        public PersistentId PersistentId;
        public AssetId EnemyId;
        public int KillCount;
    }
}